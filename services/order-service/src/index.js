const express = require('express');
const cors = require('cors');
const morgan = require('morgan');
const bodyParser = require('body-parser');
const { Pool } = require('pg');
const { v4: uuidv4 } = require('uuid');
const amqp = require('amqplib');
const axios = require('axios');

const app = express();
const PORT = process.env.PORT || 3003;
const RABBITMQ_URL = process.env.RABBITMQ_URL || 'amqp://localhost';
const PAYMENT_SERVICE_URL = process.env.PAYMENT_SERVICE_URL || 'http://localhost:3004';
const PRODUCT_SERVICE_URL = process.env.PRODUCT_SERVICE_URL || 'http://localhost:3002';

// 中間件
app.use(cors());
app.use(morgan('dev'));
app.use(bodyParser.json());

// PostgreSQL 連接
const pool = new Pool({
  user: process.env.DB_USER || 'postgres',
  host: process.env.DB_HOST || 'localhost',
  database: process.env.DB_NAME || 'order_service',
  password: process.env.DB_PASSWORD || 'postgres',
  port: process.env.DB_PORT || 5432,
});

// RabbitMQ 連接
let channel;
const connectRabbitMQ = async () => {
  try {
    const connection = await amqp.connect(RABBITMQ_URL);
    channel = await connection.createChannel();
    
    // 確保訂單事件隊列存在
    await channel.assertQueue('order_events', { durable: true });
    
    // 監聽支付事件
    await channel.assertQueue('payment_events', { durable: true });
    channel.consume('payment_events', handlePaymentEvent, { noAck: false });
    
    console.log('RabbitMQ 連接成功');
  } catch (error) {
    console.error('RabbitMQ 連接失敗:', error);
    console.log('繼續啟動服務，但異步消息功能將不可用');
  }
};

// 處理支付事件
const handlePaymentEvent = async (msg) => {
  try {
    const event = JSON.parse(msg.content.toString());
    console.log('收到支付事件:', event.type);
    
    const client = await pool.connect();
    
    try {
      await client.query('BEGIN');
      
      if (event.type === 'payment_completed') {
        // 更新訂單狀態為已支付
        await client.query(
          `UPDATE orders SET status = 'paid', updated_at = CURRENT_TIMESTAMP WHERE id = $1`,
          [event.data.orderId]
        );
        
        // 記錄訂單狀態變更
        await client.query(
          `INSERT INTO order_status_history (id, order_id, status, comment, changed_by) 
           VALUES ($1, $2, $3, $4, $5)`,
          [uuidv4(), event.data.orderId, 'paid', '支付完成', 'system']
        );
        
        await client.query('COMMIT');
        
        // 發送訂單已支付事件
        if (channel) {
          channel.sendToQueue(
            'order_events',
            Buffer.from(JSON.stringify({
              type: 'order_paid',
              data: {
                orderId: event.data.orderId,
                userId: event.data.userId
              }
            })),
            { persistent: true }
          );
        }
      } else if (event.type === 'payment_failed') {
        // 更新訂單狀態為支付失敗
        await client.query(
          `UPDATE orders SET status = 'payment_failed', updated_at = CURRENT_TIMESTAMP WHERE id = $1`,
          [event.data.orderId]
        );
        
        // 記錄訂單狀態變更
        await client.query(
          `INSERT INTO order_status_history (id, order_id, status, comment, changed_by) 
           VALUES ($1, $2, $3, $4, $5)`,
          [uuidv4(), event.data.orderId, 'payment_failed', '支付失敗', 'system']
        );
        
        await client.query('COMMIT');
      }
      
      channel.ack(msg);
    } catch (error) {
      await client.query('ROLLBACK');
      console.error('處理支付事件錯誤:', error);
      channel.nack(msg, false, true); // 重新入隊
    } finally {
      client.release();
    }
  } catch (error) {
    console.error('解析支付事件錯誤:', error);
    channel.nack(msg, false, false); // 拒絕並丟棄
  }
};

// 驗證 JWT 中間件
const authenticateToken = (req, res, next) => {
  // 簡化版，實際應該調用認證服務或驗證 JWT
  const authHeader = req.headers['authorization'];
  if (!authHeader) {
    return res.status(401).json({ message: '未提供認證令牌' });
  }
  
  // 從令牌中提取用戶 ID
  req.userId = 'user-123'; // 模擬用戶 ID
  next();
};

// 購物車路由
app.post('/api/carts', async (req, res) => {
  try {
    const { userId, sessionId } = req.body;
    
    const result = await pool.query(
      'INSERT INTO carts (user_id, session_id, status) VALUES ($1, $2, $3) RETURNING *',
      [userId, sessionId || uuidv4(), 'active']
    );
    
    res.status(201).json(result.rows[0]);
  } catch (error) {
    console.error('創建購物車錯誤:', error);
    res.status(500).json({ message: '服務器錯誤' });
  }
});

app.post('/api/carts/:cartId/items', async (req, res) => {
  try {
    const { productId, variantId, quantity, unitPrice, name, attributes } = req.body;
    const { cartId } = req.params;
    
    // 檢查購物車是否存在
    const cartResult = await pool.query('SELECT * FROM carts WHERE id = $1', [cartId]);
    if (cartResult.rows.length === 0) {
      return res.status(404).json({ message: '購物車不存在' });
    }
    
    // 檢查商品是否存在和庫存（實際應調用商品服務）
    try {
      const productResponse = await axios.get(`${PRODUCT_SERVICE_URL}/api/products/${productId}`);
      const product = productResponse.data;
      
      if (product.stock < quantity) {
        return res.status(400).json({ message: '商品庫存不足' });
      }
    } catch (error) {
      console.error('檢查商品錯誤:', error);
      // 在開發環境中，可以忽略商品服務不可用的情況
    }
    
    const result = await pool.query(
      `INSERT INTO cart_items 
       (cart_id, product_id, variant_id, quantity, unit_price, name, attributes) 
       VALUES ($1, $2, $3, $4, $5, $6, $7) 
       RETURNING *`,
      [cartId, productId, variantId, quantity, unitPrice, name, attributes]
    );
    
    res.status(201).json(result.rows[0]);
  } catch (error) {
    console.error('添加購物車項目錯誤:', error);
    res.status(500).json({ message: '服務器錯誤' });
  }
});

// 訂單路由
app.post('/api/orders', authenticateToken, async (req, res) => {
  const client = await pool.connect();
  
  try {
    await client.query('BEGIN');
    
    const { cartId, shippingAddressId, billingAddressId, shippingMethod } = req.body;
    const userId = req.userId;
    
    // 檢查購物車是否存在並獲取項目
    const cartItemsResult = await client.query(
      'SELECT * FROM cart_items WHERE cart_id = $1',
      [cartId]
    );
    
    if (cartItemsResult.rows.length === 0) {
      await client.query('ROLLBACK');
      return res.status(400).json({ message: '購物車為空' });
    }
    
    // 計算訂單總金額和項目數量
    const cartItems = cartItemsResult.rows;
    const itemsCount = cartItems.length;
    const totalAmount = cartItems.reduce((sum, item) => sum + (item.unit_price * item.quantity), 0);
    
    // 創建訂單
    const orderId = uuidv4();
    const orderNumber = `ORD-${Date.now()}`;
    
    const orderResult = await client.query(
      `INSERT INTO orders 
       (id, order_number, user_id, status, total_amount, items_count, 
        shipping_address_id, billing_address_id, shipping_method) 
       VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9) 
       RETURNING *`,
      [orderId, orderNumber, userId, 'pending', totalAmount, itemsCount, 
       shippingAddressId, billingAddressId, shippingMethod]
    );
    
    // 創建訂單項目
    for (const item of cartItems) {
      await client.query(
        `INSERT INTO order_items 
         (order_id, product_id, variant_id, name, quantity, unit_price, total_price, attributes) 
         VALUES ($1, $2, $3, $4, $5, $6, $7, $8)`,
        [orderId, item.product_id, item.variant_id, item.name, item.quantity, 
         item.unit_price, item.unit_price * item.quantity, item.attributes]
      );
    }
    
    // 記錄訂單狀態歷史
    await client.query(
      `INSERT INTO order_status_history 
       (id, order_id, status, comment, changed_by) 
       VALUES ($1, $2, $3, $4, $5)`,
      [uuidv4(), orderId, 'pending', '訂單已創建', 'system']
    );
    
    // 更新購物車狀態
    await client.query(
      `UPDATE carts SET status = 'converted', updated_at = CURRENT_TIMESTAMP WHERE id = $1`,
      [cartId]
    );
    
    await client.query('COMMIT');
    
    // 發送訂單創建事件
    if (channel) {
      channel.sendToQueue(
        'order_events',
        Buffer.from(JSON.stringify({
          type: 'order_created',
          data: {
            orderId,
            userId,
            totalAmount,
            items: cartItems.map(item => ({
              productId: item.product_id,
              variantId: item.variant_id,
              quantity: item.quantity
            }))
          }
        })),
        { persistent: true }
      );
    }
    
    res.status(201).json({
      id: orderId,
      orderNumber,
      status: 'pending',
      totalAmount,
      itemsCount,
      message: '訂單已創建，等待支付'
    });
  } catch (error) {
    await client.query('ROLLBACK');
    console.error('創建訂單錯誤:', error);
    res.status(500).json({ message: '服務器錯誤' });
  } finally {
    client.release();
  }
});

// 獲取訂單
app.get('/api/orders/:id', authenticateToken, async (req, res) => {
  try {
    const { id } = req.params;
    const userId = req.userId;
    
    // 獲取訂單信息
    const orderResult = await pool.query(
      'SELECT * FROM orders WHERE id = $1 AND user_id = $2',
      [id, userId]
    );
    
    if (orderResult.rows.length === 0) {
      return res.status(404).json({ message: '訂單不存在' });
    }
    
    const order = orderResult.rows[0];
    
    // 獲取訂單項目
    const itemsResult = await pool.query(
      'SELECT * FROM order_items WHERE order_id = $1',
      [id]
    );
    
    // 獲取訂單狀態歷史
    const historyResult = await pool.query(
      'SELECT * FROM order_status_history WHERE order_id = $1 ORDER BY changed_at DESC',
      [id]
    );
    
    res.json({
      ...order,
      items: itemsResult.rows,
      statusHistory: historyResult.rows
    });
  } catch (error) {
    console.error('獲取訂單錯誤:', error);
    res.status(500).json({ message: '服務器錯誤' });
  }
});

// 支付訂單
app.post('/api/orders/:id/pay', authenticateToken, async (req, res) => {
  try {
    const { id } = req.params;
    const { paymentMethodId } = req.body;
    const userId = req.userId;
    
    // 獲取訂單信息
    const orderResult = await pool.query(
      'SELECT * FROM orders WHERE id = $1 AND user_id = $2',
      [id, userId]
    );
    
    if (orderResult.rows.length === 0) {
      return res.status(404).json({ message: '訂單不存在' });
    }
    
    const order = orderResult.rows[0];
    
    if (order.status !== 'pending') {
      return res.status(400).json({ message: `訂單狀態為 ${order.status}，無法支付` });
    }
    
    // 創建支付交易（調用支付服務）
    try {
      const paymentResponse = await axios.post(`${PAYMENT_SERVICE_URL}/api/payment-transactions`, {
        orderId: id,
        paymentMethodId,
        amount: order.total_amount,
        currency: 'TWD'
      }, {
        headers: {
          'Authorization': req.headers.authorization
        }
      });
      
      res.json({
        orderId: id,
        paymentTransaction: paymentResponse.data,
        message: '支付交易已創建'
      });
    } catch (error) {
      console.error('創建支付交易錯誤:', error.response?.data || error.message);
      res.status(error.response?.status || 500).json({ 
        message: error.response?.data?.message || '創建支付交易失敗' 
      });
    }
  } catch (error) {
    console.error('支付訂單錯誤:', error);
    res.status(500).json({ message: '服務器錯誤' });
  }
});

// 啟動服務器
const startServer = async () => {
  try {
    await connectRabbitMQ();
    
    app.listen(PORT, () => {
      console.log(`訂單服務運行在端口 ${PORT}`);
    });
  } catch (error) {
    console.error('啟動服務器錯誤:', error);
    process.exit(1);
  }
};

startServer();