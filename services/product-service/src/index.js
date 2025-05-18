const express = require('express');
const cors = require('cors');
const morgan = require('morgan');
const bodyParser = require('body-parser');
const { MongoClient, ObjectId } = require('mongodb');
const { v4: uuidv4 } = require('uuid');
const amqp = require('amqplib');

const app = express();
const PORT = process.env.PORT || 3002;
const MONGO_URI = process.env.MONGO_URI || 'mongodb://localhost:27017/product_service';
const RABBITMQ_URL = process.env.RABBITMQ_URL || 'amqp://localhost';

// 中間件
app.use(cors());
app.use(morgan('dev'));
app.use(bodyParser.json());

// MongoDB 連接
let db;
const connectDB = async () => {
  try {
    const client = await MongoClient.connect(MONGO_URI);
    db = client.db();
    console.log('MongoDB 連接成功');
    
    // 確保索引存在
    await db.collection('products').createIndex({ name: 'text', description: 'text' });
    await db.collection('products').createIndex({ category: 1 });
    await db.collection('products').createIndex({ 'variants.sku': 1 });
  } catch (error) {
    console.error('MongoDB 連接失敗:', error);
    process.exit(1);
  }
};

// RabbitMQ 連接
let channel;
const connectRabbitMQ = async () => {
  try {
    const connection = await amqp.connect(RABBITMQ_URL);
    channel = await connection.createChannel();
    
    // 監聽訂單事件
    await channel.assertQueue('order_events', { durable: true });
    channel.consume('order_events', handleOrderEvent, { noAck: false });
    
    console.log('RabbitMQ 連接成功');
  } catch (error) {
    console.error('RabbitMQ 連接失敗:', error);
    console.log('繼續啟動服務，但異步消息功能將不可用');
  }
};

// 處理訂單事件
const handleOrderEvent = async (msg) => {
  try {
    const event = JSON.parse(msg.content.toString());
    console.log('收到訂單事件:', event.type);
    
    if (event.type === 'order_created') {
      // 更新商品庫存
      for (const item of event.data.items) {
        try {
          const result = await db.collection('products').updateOne(
            { _id: item.productId },
            { $inc: { stock: -item.quantity } }
          );
          
          if (result.modifiedCount === 0) {
            console.error(`無法更新商品庫存: ${item.productId}`);
          }
        } catch (error) {
          console.error('更新商品庫存錯誤:', error);
        }
      }
      
      channel.ack(msg);
    } else {
      // 不關心的事件類型
      channel.ack(msg);
    }
  } catch (error) {
    console.error('處理訂單事件錯誤:', error);
    channel.nack(msg, false, true); // 重新入隊
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

// 產品路由
app.get('/api/products', async (req, res) => {
  try {
    const { category, search, page = 1, limit = 10 } = req.query;
    const skip = (page - 1) * parseInt(limit);
    
    let query = {};
    
    if (category) {
      query.category = category;
    }
    
    if (search) {
      query.$text = { $search: search };
    }
    
    const products = await db.collection('products')
      .find(query)
      .skip(skip)
      .limit(parseInt(limit))
      .toArray();
      
    const total = await db.collection('products').countDocuments(query);
    
    res.json({
      products,
      pagination: {
        total,
        page: parseInt(page),
        limit: parseInt(limit),
        pages: Math.ceil(total / limit)
      }
    });
  } catch (error) {
    console.error('獲取產品錯誤:', error);
    res.status(500).json({ message: '服務器錯誤' });
  }
});

app.get('/api/products/:id', async (req, res) => {
  try {
    const product = await db.collection('products').findOne({ _id: req.params.id });
    
    if (!product) {
      return res.status(404).json({ message: '產品不存在' });
    }
    
    res.json(product);
  } catch (error) {
    console.error('獲取產品詳情錯誤:', error);
    res.status(500).json({ message: '服務器錯誤' });
  }
});

app.post('/api/products', authenticateToken, async (req, res) => {
  try {
    const { name, description, price, category, stock, images, variants } = req.body;
    
    const newProduct = {
      _id: uuidv4(),
      name,
      description,
      price,
      category,
      stock: stock || 0,
      images: images || [],
      variants: variants || [],
      createdAt: new Date(),
      updatedAt: new Date()
    };
    
    await db.collection('products').insertOne(newProduct);
    
    res.status(201).json(newProduct);
  } catch (error) {
    console.error('創建產品錯誤:', error);
    res.status(500).json({ message: '服務器錯誤' });
  }
});

app.put('/api/products/:id', authenticateToken, async (req, res) => {
  try {
    const { name, description, price, category, stock, images, variants } = req.body;
    
    const updatedProduct = {
      name,
      description,
      price,
      category,
      stock,
      images: images || [],
      variants: variants || [],
      updatedAt: new Date()
    };
    
    const result = await db.collection('products').updateOne(
      { _id: req.params.id },
      { $set: updatedProduct }
    );
    
    if (result.matchedCount === 0) {
      return res.status(404).json({ message: '產品不存在' });
    }
    
    res.json({ ...updatedProduct, _id: req.params.id });
  } catch (error) {
    console.error('更新產品錯誤:', error);
    res.status(500).json({ message: '服務器錯誤' });
  }
});

app.delete('/api/products/:id', authenticateToken, async (req, res) => {
  try {
    const result = await db.collection('products').deleteOne({ _id: req.params.id });
    
    if (result.deletedCount === 0) {
      return res.status(404).json({ message: '產品不存在' });
    }
    
    res.json({ message: '產品已刪除' });
  } catch (error) {
    console.error('刪除產品錯誤:', error);
    res.status(500).json({ message: '服務器錯誤' });
  }
});

// 獲取商品分類
app.get('/api/categories', async (req, res) => {
  try {
    const categories = await db.collection('products').distinct('category');
    res.json(categories);
  } catch (error) {
    console.error('獲取分類錯誤:', error);
    res.status(500).json({ message: '服務器錯誤' });
  }
});

// 啟動服務器
const startServer = async () => {
  try {
    await connectDB();
    await connectRabbitMQ();
    
    app.listen(PORT, () => {
      console.log(`商品服務運行在端口 ${PORT}`);
    });
  } catch (error) {
    console.error('啟動服務器錯誤:', error);
    process.exit(1);
  }
};

startServer();