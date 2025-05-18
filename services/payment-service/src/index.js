const express = require('express');
const cors = require('cors');
const morgan = require('morgan');
const bodyParser = require('body-parser');
const { Pool } = require('pg');
const { v4: uuidv4 } = require('uuid');
const amqp = require('amqplib');

const app = express();
const PORT = process.env.PORT || 3004;
const RABBITMQ_URL = process.env.RABBITMQ_URL || 'amqp://localhost';

// 中間件
app.use(cors());
app.use(morgan('dev'));
app.use(bodyParser.json());

// PostgreSQL 連接
const pool = new Pool({
  user: process.env.DB_USER || 'postgres',
  host: process.env.DB_HOST || 'localhost',
  database: process.env.DB_NAME || 'payment_service',
  password: process.env.DB_PASSWORD || 'postgres',
  port: process.env.DB_PORT || 5432,
});

// RabbitMQ 連接
let channel;
const connectRabbitMQ = async () => {
  try {
    const connection = await amqp.connect(RABBITMQ_URL);
    channel = await connection.createChannel();
    
    // 確保支付事件隊列存在
    await channel.assertQueue('payment_events', { durable: true });
    console.log('RabbitMQ 連接成功');
  } catch (error) {
    console.error('RabbitMQ 連接失敗:', error);
    // 在開發環境中，可以容忍RabbitMQ連接失敗
    console.log('繼續啟動服務，但異步消息功能將不可用');
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

// 支付方式路由
app.get('/api/payment-methods', async (req, res) => {
  try {
    const result = await pool.query('SELECT * FROM payment_methods WHERE is_active = true');
    res.json(result.rows);
  } catch (error) {
    console.error('獲取支付方式錯誤:', error);
    res.status(500).json({ message: '服務器錯誤' });
  }
});

// 創建支付交易
app.post('/api/payment-transactions', authenticateToken, async (req, res) => {
  const client = await pool.connect();
  
  try {
    await client.query('BEGIN');
    
    const { orderId, paymentMethodId, amount, currency = 'TWD' } = req.body;
    const userId = req.userId;
    
    // 驗證支付方式
    const paymentMethodResult = await client.query(
      'SELECT * FROM payment_methods WHERE id = $1 AND is_active = true',
      [paymentMethodId]
    );
    
    if (paymentMethodResult.rows.length === 0) {
      await client.query('ROLLBACK');
      return res.status(400).json({ message: '無效的支付方式' });
    }
    
    // 創建支付交易
    const transactionId = uuidv4();
    
    const result = await client.query(
      `INSERT INTO payment_transactions 
       (id, order_id, user_id, payment_method_id, amount, currency, status, client_ip) 
       VALUES ($1, $2, $3, $4, $5, $6, $7, $8) 
       RETURNING *`,
      [transactionId, orderId, userId, paymentMethodId, amount, currency, 'pending', req.ip]
    );
    
    // 記錄狀態變更
    await client.query(
      `INSERT INTO payment_status_history 
       (id, payment_transaction_id, new_status, changed_by) 
       VALUES ($1, $2, $3, $4)`,
      [uuidv4(), transactionId, 'pending', 'system']
    );
    
    await client.query('COMMIT');
    
    // 發送支付創建事件
    if (channel) {
      channel.sendToQueue(
        'payment_events',
        Buffer.from(JSON.stringify({
          type: 'payment_created',
          data: {
            transactionId,
            orderId,
            userId,
            amount,
            currency,
            status: 'pending'
          }
        })),
        { persistent: true }
      );
    }
    
    res.status(201).json({
      id: transactionId,
      orderId,
      amount,
      currency,
      status: 'pending',
      paymentUrl: `/api/payments/${transactionId}/process`
    });
  } catch (error) {
    await client.query('ROLLBACK');
    console.error('創建支付交易錯誤:', error);
    res.status(500).json({ message: '服務器錯誤' });
  } finally {
    client.release();
  }
});

// 處理支付
app.post('/api/payments/:id/process', async (req, res) => {
  const client = await pool.connect();
  
  try {
    await client.query('BEGIN');
    
    const { id } = req.params;
    const { paymentDetails } = req.body;
    
    // 獲取交易信息
    const transactionResult = await client.query(
      'SELECT * FROM payment_transactions WHERE id = $1',
      [id]
    );
    
    if (transactionResult.rows.length === 0) {
      await client.query('ROLLBACK');
      return res.status(404).json({ message: '交易不存在' });
    }
    
    const transaction = transactionResult.rows[0];
    
    if (transaction.status !== 'pending') {
      await client.query('ROLLBACK');
      return res.status(400).json({ message: `交易狀態為 ${transaction.status}，無法處理` });
    }
    
    // 模擬支付處理
    // 在實際環境中，這裡會調用支付提供商的API
    const isSuccessful = Math.random() > 0.2; // 80% 成功率
    
    if (isSuccessful) {
      // 更新交易狀態為完成
      await client.query(
        `UPDATE payment_transactions 
         SET status = 'completed', 
             completed_at = CURRENT_TIMESTAMP,
             transaction_reference = $1,
             payment_provider_response = $2
         WHERE id = $3`,
        [`ref-${Date.now()}`, JSON.stringify({ success: true }), id]
      );
      
      // 記錄狀態變更
      await client.query(
        `INSERT INTO payment_status_history 
         (id, payment_transaction_id, previous_status, new_status, changed_by) 
         VALUES ($1, $2, $3, $4, $5)`,
        [uuidv4(), id, 'pending', 'completed', 'system']
      );
      
      await client.query('COMMIT');
      
      // 發送支付完成事件
      if (channel) {
        channel.sendToQueue(
          'payment_events',
          Buffer.from(JSON.stringify({
            type: 'payment_completed',
            data: {
              transactionId: id,
              orderId: transaction.order_id,
              userId: transaction.user_id,
              amount: transaction.amount,
              currency: transaction.currency
            }
          })),
          { persistent: true }
        );
      }
      
      res.json({
        id,
        status: 'completed',
        message: '支付成功'
      });
    } else {
      // 更新交易狀態為失敗
      await client.query(
        `UPDATE payment_transactions 
         SET status = 'failed', 
             error_message = $1,
             payment_provider_response = $2
         WHERE id = $3`,
        ['支付處理失敗', JSON.stringify({ success: false }), id]
      );
      
      // 記錄狀態變更
      await client.query(
        `INSERT INTO payment_status_history 
         (id, payment_transaction_id, previous_status, new_status, changed_by) 
         VALUES ($1, $2, $3, $4, $5)`,
        [uuidv4(), id, 'pending', 'failed', 'system']
      );
      
      await client.query('COMMIT');
      
      // 發送支付失敗事件
      if (channel) {
        channel.sendToQueue(
          'payment_events',
          Buffer.from(JSON.stringify({
            type: 'payment_failed',
            data: {
              transactionId: id,
              orderId: transaction.order_id,
              userId: transaction.user_id,
              reason: '支付處理失敗'
            }
          })),
          { persistent: true }
        );
      }
      
      res.status(400).json({
        id,
        status: 'failed',
        message: '支付失敗'
      });
    }
  } catch (error) {
    await client.query('ROLLBACK');
    console.error('處理支付錯誤:', error);
    res.status(500).json({ message: '服務器錯誤' });
  } finally {
    client.release();
  }
});

// 獲取支付交易
app.get('/api/payment-transactions/:id', authenticateToken, async (req, res) => {
  try {
    const { id } = req.params;
    
    const result = await pool.query(
      'SELECT * FROM payment_transactions WHERE id = $1',
      [id]
    );
    
    if (result.rows.length === 0) {
      return res.status(404).json({ message: '交易不存在' });
    }
    
    res.json(result.rows[0]);
  } catch (error) {
    console.error('獲取支付交易錯誤:', error);
    res.status(500).json({ message: '服務器錯誤' });
  }
});

// 啟動服務器
const startServer = async () => {
  try {
    await connectRabbitMQ();
    
    app.listen(PORT, () => {
      console.log(`支付服務運行在端口 ${PORT}`);
    });
  } catch (error) {
    console.error('啟動服務器錯誤:', error);
    process.exit(1);
  }
};

startServer();