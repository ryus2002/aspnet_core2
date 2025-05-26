# 電商平台關鍵頁面設計建議與實現方案

根據您的需求，我將為登入/註冊頁面、商品列表與詳情頁以及購物車界面提供具體的設計建議和實現方案。以下建議基於現有的後端微服務架構，並採用現代前端開發最佳實踐。

## 一、登入/註冊頁面設計

### 設計建議

1. **簡潔統一的視覺風格**：
   - 使用簡潔的表單設計，減少視覺干擾
   - 配色方案與品牌一致，提高品牌識別度
   - 響應式設計，確保在不同設備上的良好體驗

2. **用戶體驗優化**：
   - 表單驗證即時反饋
   - 清晰的錯誤提示
   - 記住我功能
   - 社交媒體登入選項
   - 忘記密碼流程

3. **安全性考量**：
   - HTTPS 連接
   - 密碼強度指示器
   - 防止暴力破解的限制措施
   - CSRF 保護

### 實現方案

#### 技術選型建議：
- **前端框架**：React 或 Vue.js
- **UI 組件庫**：Ant Design 或 Material-UI
- **表單處理**：Formik 或 React Hook Form
- **表單驗證**：Yup 或 Zod

#### 登入頁面實現示例 (React + Ant Design)：

```jsx
// Login.jsx
import React, { useState } from 'react';
import { Form, Input, Button, Checkbox, Divider, message } from 'antd';
import { UserOutlined, LockOutlined } from '@ant-design/icons';
import { Link, useNavigate } from 'react-router-dom';
import { login } from '../services/authService';

const Login = () => {
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const onFinish = async (values) => {
    try {
      setLoading(true);
      const response = await login(values.username, values.password);
      // 存儲 JWT token
      localStorage.setItem('token', response.token);
      localStorage.setItem('refreshToken', response.refreshToken);
      
      message.success('登入成功！');
      navigate('/dashboard');
    } catch (error) {
      message.error(error.message || '登入失敗，請檢查您的憑證');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-form-wrapper">
        <h1 className="login-title">歡迎回來</h1>
        <p className="login-subtitle">請登入您的帳戶</p>
        
        <Form
          name="login_form"
          className="login-form"
          initialValues={{ remember: true }}
          onFinish={onFinish}
        >
          <Form.Item
            name="username"
            rules={[{ required: true, message: '請輸入您的用戶名!' }]}
          >
            <Input 
              prefix={<UserOutlined />} 
              placeholder="用戶名" 
              size="large" 
            />
          </Form.Item>
          
          <Form.Item
            name="password"
            rules={[{ required: true, message: '請輸入您的密碼!' }]}
          >
            <Input.Password
              prefix={<LockOutlined />}
              placeholder="密碼"
              size="large"
            />
          </Form.Item>
          
          <Form.Item>
            <Form.Item name="remember" valuePropName="checked" noStyle>
              <Checkbox>記住我</Checkbox>
            </Form.Item>
            <Link className="login-form-forgot" to="/forgot-password">
              忘記密碼
            </Link>
          </Form.Item>

          <Form.Item>
            <Button 
              type="primary" 
              htmlType="submit" 
              className="login-form-button"
              loading={loading}
              block
              size="large"
            >
              登入
            </Button>
          </Form.Item>
          
          <Divider>或</Divider>
          
          <div className="social-login">
            <Button className="social-btn google">Google 登入</Button>
            <Button className="social-btn facebook">Facebook 登入</Button>
          </div>
          
          <div className="register-link">
            還沒有帳戶？ <Link to="/register">立即註冊</Link>
          </div>
        </Form>
      </div>
    </div>
  );
};

export default Login;
```

#### 認證服務實現：

```jsx
// authService.js
import axios from 'axios';

const API_URL = 'https://api-gateway/auth';

export const login = async (username, password) => {
  try {
    const response = await axios.post(`${API_URL}/login`, {
      username,
      password
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '登入失敗，請稍後再試'
    );
  }
};

export const register = async (userData) => {
  try {
    const response = await axios.post(`${API_URL}/register`, userData);
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '註冊失敗，請稍後再試'
    );
  }
};

export const refreshToken = async (refreshToken) => {
  try {
    const response = await axios.post(`${API_URL}/refresh-token`, {
      refreshToken
    });
    return response.data;
  } catch (error) {
    throw new Error('無法刷新令牌');
  }
};
```

## 二、商品列表與詳情頁設計

### 設計建議

1. **商品列表頁**：
   - 網格與列表視圖切換選項
   - 強大的篩選與排序功能
   - 分頁或無限滾動加載
   - 快速預覽功能
   - 願望清單/收藏功能
   - 價格範圍篩選

2. **商品詳情頁**：
   - 高質量商品圖片輪播
   - 詳細的商品描述與規格
   - 客戶評價與評分
   - 相關商品推薦
   - 清晰的價格與庫存信息
   - 尺寸/顏色等選項（如適用）
   - 加入購物車按鈕突出顯示

### 實現方案

#### 商品列表頁實現示例 (React + Ant Design)：

```jsx
// ProductList.jsx
import React, { useState, useEffect } from 'react';
import { Row, Col, Card, Input, Select, Pagination, Slider, Tag, Spin, Empty } from 'antd';
import { ShoppingCartOutlined, HeartOutlined, FilterOutlined, AppstoreOutlined, BarsOutlined } from '@ant-design/icons';
import { Link } from 'react-router-dom';
import { getProducts } from '../services/productService';

const { Search } = Input;
const { Option } = Select;
const { Meta } = Card;

const ProductList = () => {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [viewMode, setViewMode] = useState('grid'); // 'grid' or 'list'
  const [filters, setFilters] = useState({
    category: 'all',
    priceRange: [0, 10000],
    sortBy: 'newest'
  });
  const [pagination, setPagination] = useState({
    current: 1,
    pageSize: 12,
    total: 0
  });

  useEffect(() => {
    fetchProducts();
  }, [filters, pagination.current]);

  const fetchProducts = async () => {
    try {
      setLoading(true);
      const response = await getProducts({
        page: pagination.current,
        pageSize: pagination.pageSize,
        category: filters.category === 'all' ? null : filters.category,
        minPrice: filters.priceRange[0],
        maxPrice: filters.priceRange[1],
        sortBy: filters.sortBy
      });
      
      setProducts(response.items);
      setPagination({
        ...pagination,
        total: response.total
      });
    } catch (error) {
      console.error('Failed to fetch products:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCategoryChange = (value) => {
    setFilters({
      ...filters,
      category: value
    });
    setPagination({
      ...pagination,
      current: 1
    });
  };

  const handleSortChange = (value) => {
    setFilters({
      ...filters,
      sortBy: value
    });
  };

  const handlePriceRangeChange = (value) => {
    setFilters({
      ...filters,
      priceRange: value
    });
  };

  const handleSearch = (value) => {
    // 實現搜索邏輯
  };

  const handlePageChange = (page) => {
    setPagination({
      ...pagination,
      current: page
    });
  };

  const renderProductGrid = () => {
    if (products.length === 0 && !loading) {
      return <Empty description="沒有找到符合條件的商品" />;
    }

    return (
      <Row gutter={[16, 16]}>
        {products.map(product => (
          <Col xs={24} sm={12} md={8} lg={6} key={product.id}>
            <Card
              hoverable
              cover={<img alt={product.name} src={product.imageUrl} />}
              actions={[
                <HeartOutlined key="favorite" />,
                <ShoppingCartOutlined key="add-to-cart" />
              ]}
            >
              <Meta 
                title={<Link to={`/products/${product.id}`}>{product.name}</Link>} 
                description={
                  <>
                    <div className="product-price">NT$ {product.price.toLocaleString()}</div>
                    {product.discount > 0 && (
                      <Tag color="red">節省 {product.discount}%</Tag>
                    )}
                  </>
                }
              />
            </Card>
          </Col>
        ))}
      </Row>
    );
  };

  return (
    <div className="product-list-container">
      <div className="product-list-header">
        <h1>所有商品</h1>
        <div className="product-list-tools">
          <Search
            placeholder="搜尋商品"
            allowClear
            onSearch={handleSearch}
            style={{ width: 200 }}
          />
          
          <Select 
            defaultValue="all" 
            style={{ width: 120 }} 
            onChange={handleCategoryChange}
          >
            <Option value="all">所有類別</Option>
            <Option value="electronics">電子產品</Option>
            <Option value="clothing">服飾</Option>
            <Option value="home">家居</Option>
          </Select>
          
          <Select
            defaultValue="newest"
            style={{ width: 120 }}
            onChange={handleSortChange}
          >
            <Option value="newest">最新上架</Option>
            <Option value="price-asc">價格由低到高</Option>
            <Option value="price-desc">價格由高到低</Option>
            <Option value="popular">熱門商品</Option>
          </Select>
          
          <div className="view-mode-toggle">
            <AppstoreOutlined 
              className={viewMode === 'grid' ? 'active' : ''} 
              onClick={() => setViewMode('grid')} 
            />
            <BarsOutlined 
              className={viewMode === 'list' ? 'active' : ''} 
              onClick={() => setViewMode('list')} 
            />
          </div>
        </div>
      </div>
      
      <div className="product-list-content">
        <div className="product-list-sidebar">
          <div className="filter-section">
            <h3><FilterOutlined /> 價格範圍</h3>
            <Slider
              range
              min={0}
              max={10000}
              defaultValue={[0, 10000]}
              onChange={handlePriceRangeChange}
            />
            <div className="price-range-display">
              NT$ {filters.priceRange[0].toLocaleString()} - NT$ {filters.priceRange[1].toLocaleString()}
            </div>
          </div>
          
          {/* 其他篩選選項可在此添加 */}
        </div>
        
        <div className="product-list-main">
          {loading ? (
            <div className="loading-container">
              <Spin size="large" />
            </div>
          ) : (
            renderProductGrid()
          )}
          
          <div className="pagination-container">
            <Pagination
              current={pagination.current}
              pageSize={pagination.pageSize}
              total={pagination.total}
              onChange={handlePageChange}
              showSizeChanger={false}
            />
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProductList;
```

#### 商品詳情頁實現示例：

```jsx
// ProductDetail.jsx
import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { Row, Col, Carousel, Button, InputNumber, Tabs, Rate, Comment, Avatar, List, Spin, message } from 'antd';
import { ShoppingCartOutlined, HeartOutlined, ShareAltOutlined, CheckCircleOutlined } from '@ant-design/icons';
import { getProductById, getProductReviews, getRelatedProducts } from '../services/productService';
import { addToCart } from '../services/cartService';

const { TabPane } = Tabs;

const ProductDetail = () => {
  const { id } = useParams();
  const [product, setProduct] = useState(null);
  const [reviews, setReviews] = useState([]);
  const [relatedProducts, setRelatedProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [quantity, setQuantity] = useState(1);
  const [selectedVariant, setSelectedVariant] = useState(null);
  
  useEffect(() => {
    fetchProductDetails();
  }, [id]);
  
  const fetchProductDetails = async () => {
    try {
      setLoading(true);
      const productData = await getProductById(id);
      setProduct(productData);
      
      // 如果有變體，默認選擇第一個
      if (productData.variants && productData.variants.length > 0) {
        setSelectedVariant(productData.variants[0]);
      }
      
      // 獲取評論
      const reviewsData = await getProductReviews(id);
      setReviews(reviewsData);
      
      // 獲取相關商品
      const relatedData = await getRelatedProducts(id);
      setRelatedProducts(relatedData);
    } catch (error) {
      console.error('Failed to fetch product details:', error);
      message.error('無法載入商品詳情，請稍後再試');
    } finally {
      setLoading(false);
    }
  };
  
  const handleAddToCart = async () => {
    try {
      await addToCart({
        productId: id,
        variantId: selectedVariant?.id,
        quantity: quantity
      });
      message.success('商品已加入購物車！');
    } catch (error) {
      message.error('加入購物車失敗，請稍後再試');
    }
  };
  
  const handleQuantityChange = (value) => {
    setQuantity(value);
  };
  
  const handleVariantSelect = (variant) => {
    setSelectedVariant(variant);
  };
  
  if (loading) {
    return (
      <div className="loading-container">
        <Spin size="large" />
      </div>
    );
  }
  
  if (!product) {
    return <div>商品不存在或已被移除</div>;
  }
  
  return (
    <div className="product-detail-container">
      <Row gutter={[32, 32]}>
        <Col xs={24} md={12}>
          <Carousel autoplay className="product-carousel">
            {product.images.map((image, index) => (
              <div key={index}>
                <img src={image} alt={`${product.name} - ${index + 1}`} />
              </div>
            ))}
          </Carousel>
          
          <div className="product-thumbnails">
            {product.images.map((image, index) => (
              <div key={index} className="thumbnail">
                <img src={image} alt={`thumbnail-${index}`} />
              </div>
            ))}
          </div>
        </Col>
        
        <Col xs={24} md={12}>
          <div className="product-info">
            <h1 className="product-name">{product.name}</h1>
            
            <div className="product-rating">
              <Rate disabled defaultValue={product.rating} />
              <span className="review-count">({product.reviewCount} 評價)</span>
            </div>
            
            <div className="product-price">
              {product.discountPrice ? (
                <>
                  <span className="original-price">NT$ {product.price.toLocaleString()}</span>
                  <span className="discount-price">NT$ {product.discountPrice.toLocaleString()}</span>
                  <span className="discount-tag">節省 {Math.round((1 - product.discountPrice / product.price) * 100)}%</span>
                </>
              ) : (
                <span className="current-price">NT$ {product.price.toLocaleString()}</span>
              )}
            </div>
            
            {product.variants && product.variants.length > 0 && (
              <div className="product-variants">
                <h3>選擇款式：</h3>
                <div className="variant-options">
                  {product.variants.map(variant => (
                    <div 
                      key={variant.id} 
                      className={`variant-option ${selectedVariant?.id === variant.id ? 'selected' : ''}`}
                      onClick={() => handleVariantSelect(variant)}
                    >
                      {variant.name}
                    </div>
                  ))}
                </div>
              </div>
            )}
            
            <div className="product-actions">
              <div className="quantity-selector">
                <span>數量：</span>
                <InputNumber 
                  min={1} 
                  max={product.stock} 
                  defaultValue={1} 
                  onChange={handleQuantityChange} 
                />
              </div>
              
              <div className="stock-info">
                {product.stock > 0 ? (
                  <span className="in-stock"><CheckCircleOutlined /> 有庫存 ({product.stock})</span>
                ) : (
                  <span className="out-of-stock">缺貨中</span>
                )}
              </div>
              
              <div className="action-buttons">
                <Button 
                  type="primary" 
                  icon={<ShoppingCartOutlined />} 
                  size="large"
                  onClick={handleAddToCart}
                  disabled={product.stock === 0}
                >
                  加入購物車
                </Button>
                
                <Button icon={<HeartOutlined />} size="large">
                  加入收藏
                </Button>
                
                <Button icon={<ShareAltOutlined />} size="large">
                  分享
                </Button>
              </div>
            </div>
            
            <div className="product-meta">
              <p><strong>商品編號：</strong> {product.sku}</p>
              <p><strong>類別：</strong> {product.category}</p>
              <p><strong>標籤：</strong> {product.tags.join(', ')}</p>
            </div>
          </div>
        </Col>
      </Row>
      
      <div className="product-details-tabs">
        <Tabs defaultActiveKey="description">
          <TabPane tab="商品描述" key="description">
            <div dangerouslySetInnerHTML={{ __html: product.description }} />
          </TabPane>
          
          <TabPane tab="規格" key="specifications">
            <table className="specifications-table">
              <tbody>
                {Object.entries(product.specifications).map(([key, value]) => (
                  <tr key={key}>
                    <td>{key}</td>
                    <td>{value}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </TabPane>
          
          <TabPane tab={`評價 (${reviews.length})`} key="reviews">
            <div className="reviews-summary">
              <div className="average-rating">
                <h2>{product.rating.toFixed(1)}</h2>
                <Rate disabled defaultValue={product.rating} />
                <p>{product.reviewCount} 位顧客評價</p>
              </div>
              
              <div className="rating-breakdown">
                {/* 評分分佈圖表 */}
              </div>
            </div>
            
            <List
              className="review-list"
              itemLayout="horizontal"
              dataSource={reviews}
              renderItem={review => (
                <Comment
                  author={review.userName}
                  avatar={<Avatar src={review.userAvatar} alt={review.userName} />}
                  content={review.content}
                  datetime={new Date(review.createdAt).toLocaleDateString()}
                  actions={[
                    <Rate disabled defaultValue={review.rating} />
                  ]}
                />
              )}
            />
          </TabPane>
        </Tabs>
      </div>
      
      <div className="related-products">
        <h2>相關商品</h2>
        <Row gutter={[16, 16]}>
          {relatedProducts.map(item => (
            <Col xs={12} sm={8} md={6} lg={4} key={item.id}>
              <Link to={`/products/${item.id}`}>
                <div className="related-product-item">
                  <img src={item.imageUrl} alt={item.name} />
                  <h4>{item.name}</h4>
                  <p>NT$ {item.price.toLocaleString()}</p>
                </div>
              </Link>
            </Col>
          ))}
        </Row>
      </div>
    </div>
  );
};

export default ProductDetail;
```

## 三、購物車界面設計

### 設計建議

1. **清晰的購物車內容展示**：
   - 商品圖片、名稱、價格、數量
   - 小計與總計金額
   - 可調整數量或移除商品
   - 保存購物車功能

2. **購物流程優化**：
   - 繼續購物與結帳按鈕
   - 購物進度指示器
   - 購物車為空時的友好提示
   - 優惠券/折扣碼輸入區域

3. **額外功能**：
   - 相關商品推薦
   - 庫存狀態顯示
   - 保存為願望清單選項
   - 運費計算器

### 實現方案

#### 購物車頁面實現示例 (React + Ant Design)：

```jsx
// Cart.jsx
import React, { useState, useEffect } from 'react';
import { Table, Button, InputNumber, Empty, Steps, Card, Input, Row, Col, Divider, message, Spin } from 'antd';
import { ShoppingOutlined, DeleteOutlined, RightOutlined, LeftOutlined, ShoppingCartOutlined } from '@ant-design/icons';
import { Link, useNavigate } from 'react-router-dom';
import { getCartItems, updateCartItem, removeCartItem, applyPromoCode } from '../services/cartService';

const { Step } = Steps;

const Cart = () => {
  const [cartItems, setCartItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [promoCode, setPromoCode] = useState('');
  const [discount, setDiscount] = useState(0);
  const [applyingPromo, setApplyingPromo] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    fetchCartItems();
  }, []);

  const fetchCartItems = async () => {
    try {
      setLoading(true);
      const response = await getCartItems();
      setCartItems(response.items);
      setDiscount(response.discount || 0);
    } catch (error) {
      console.error('Failed to fetch cart items:', error);
      message.error('無法載入購物車，請稍後再試');
    } finally {
      setLoading(false);
    }
  };

  const handleQuantityChange = async (itemId, quantity) => {
    try {
      await updateCartItem(itemId, quantity);
      setCartItems(cartItems.map(item => 
        item.id === itemId ? { ...item, quantity } : item
      ));
      recalculateTotal();
    } catch (error) {
      message.error('更新數量失敗，請稍後再試');
    }
  };

  const handleRemoveItem = async (itemId) => {
    try {
      await removeCartItem(itemId);
      setCartItems(cartItems.filter(item => item.id !== itemId));
      recalculateTotal();
      message.success('商品已從購物車移除');
    } catch (error) {
      message.error('移除商品失敗，請稍後再試');
    }
  };

  const handleApplyPromoCode = async () => {
    if (!promoCode.trim()) {
      message.warning('請輸入優惠碼');
      return;
    }

    try {
      setApplyingPromo(true);
      const response = await applyPromoCode(promoCode);
      setDiscount(response.discount);
      message.success(`優惠碼套用成功，已折抵 NT$ ${response.discount.toLocaleString()}`);
    } catch (error) {
      message.error(error.message || '優惠碼無效或已過期');
    } finally {
      setApplyingPromo(false);
    }
  };

  const recalculateTotal = () => {
    // 在實際應用中，這可能會由後端計算
    // 這裡僅為前端示範
  };

  const handleCheckout = () => {
    navigate('/checkout');
  };

  const columns = [
    {
      title: '商品',
      dataIndex: 'product',
      key: 'product',
      render: (text, record) => (
        <div className="cart-product">
          <img src={record.imageUrl} alt={record.name} className="cart-product-image" />
          <div className="cart-product-info">
            <Link to={`/products/${record.productId}`} className="cart-product-name">
              {record.name}
            </Link>
            {record.variant && <div className="cart-product-variant">款式: {record.variant}</div>}
            {!record.inStock && <div className="out-of-stock-warning">缺貨</div>}
          </div>
        </div>
      ),
    },
    {
      title: '單價',
      dataIndex: 'price',
      key: 'price',
      render: (price) => `NT$ ${price.toLocaleString()}`,
    },
    {
      title: '數量',
      key: 'quantity',
      render: (text, record) => (
        <InputNumber
          min={1}
          max={record.inStock ? record.stockQuantity : record.quantity}
          value={record.quantity}
          onChange={(value) => handleQuantityChange(record.id, value)}
          disabled={!record.inStock}
        />
      ),
    },
    {
      title: '小計',
      key: 'subtotal',
      render: (text, record) => `NT$ ${(record.price * record.quantity).toLocaleString()}`,
    },
    {
      title: '',
      key: 'action',
      render: (text, record) => (
        <Button
          type="text"
          danger
          icon={<DeleteOutlined />}
          onClick={() => handleRemoveItem(record.id)}
        >
          移除
        </Button>
      ),
    },
  ];

  const calculateSubtotal = () => {
    return cartItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);
  };

  const calculateTotal = () => {
    return calculateSubtotal() - discount;
  };

  if (loading) {
    return (
      <div className="loading-container">
        <Spin size="large" />
      </div>
    );
  }

  if (cartItems.length === 0) {
    return (
      <div className="empty-cart-container">
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="您的購物車是空的"
        >
          <Button type="primary" icon={<ShoppingOutlined />}>
            <Link to="/products">去購物</Link>
          </Button>
        </Empty>
      </div>
    );
  }

  return (
    <div className="cart-container">
      <h1 className="page-title">購物車</h1>
      
      <Steps current={0} className="checkout-steps">
        <Step title="購物車" icon={<ShoppingCartOutlined />} />
        <Step title="結帳" />
        <Step title="完成" />
      </Steps>
      
      <div className="cart-content">
        <Table
          columns={columns}
          dataSource={cartItems}
          pagination={false}
          rowKey="id"
          locale={{ emptyText: '購物車是空的' }}
        />
        
        <Row gutter={24} className="cart-summary-row">
          <Col xs={24} md={16}>
            <Card title="優惠碼" className="promo-code-card">
              <Input.Group compact>
                <Input
                  style={{ width: 'calc(100% - 120px)' }}
                  placeholder="輸入優惠碼"
                  value={promoCode}
                  onChange={(e) => setPromoCode(e.target.value)}
                />
                <Button
                  type="primary"
                  onClick={handleApplyPromoCode}
                  loading={applyingPromo}
                >
                  套用
                </Button>
              </Input.Group>
            </Card>
          </Col>
          
          <Col xs={24} md={8}>
            <Card title="訂單摘要" className="order-summary-card">
              <div className="summary-item">
                <span>小計</span>
                <span>NT$ {calculateSubtotal().toLocaleString()}</span>
              </div>
              
              {discount > 0 && (
                <div className="summary-item discount">
                  <span>折扣</span>
                  <span>-NT$ {discount.toLocaleString()}</span>
                </div>
              )}
              
              <Divider style={{ margin: '12px 0' }} />
              
              <div className="summary-item total">
                <span>總計</span>
                <span>NT$ {calculateTotal().toLocaleString()}</span>
              </div>
              
              <div className="cart-buttons">
                <Button
                  type="primary"
                  size="large"
                  block
                  onClick={handleCheckout}
                  className="checkout-button"
                >
                  前往結帳 <RightOutlined />
                </Button>
                
                <Button
                  type="link"
                  block
                  className="continue-shopping"
                >
                  <Link to="/products">
                    <LeftOutlined /> 繼續購物
                  </Link>
                </Button>
              </div>
            </Card>
          </Col>
        </Row>
      </div>
    </div>
  );
};

export default Cart;
```

## 四、前端整體架構建議

為了有效整合這些頁面，我建議以下前端架構：

### 1. 項目結構

```
src/
├── assets/              # 靜態資源
├── components/          # 共用元件
│   ├── common/          # 通用元件
│   ├── auth/            # 認證相關元件
│   ├── products/        # 商品相關元件
│   └── cart/            # 購物車相關元件
├── contexts/            # React Context
│   ├── AuthContext.js   # 認證狀態管理
│   └── CartContext.js   # 購物車狀態管理
├── hooks/               # 自定義 Hooks
├── pages/               # 頁面元件
│   ├── Login.jsx        # 登入頁面
│   ├── Register.jsx     # 註冊頁面
│   ├── ProductList.jsx  # 商品列表頁
│   ├── ProductDetail.jsx # 商品詳情頁
│   └── Cart.jsx         # 購物車頁面
├── services/            # API 服務
│   ├── authService.js   # 認證相關 API
│   ├── productService.js # 商品相關 API
│   └── cartService.js   # 購物車相關 API
├── utils/               # 工具函數
├── App.js               # 應用入口
└── index.js             # 渲染入口
```

### 2. 狀態管理

使用 React Context API 或 Redux 進行狀態管理：

```jsx
// contexts/AuthContext.js
import React, { createContext, useState, useEffect, useContext } from 'react';
import { refreshToken } from '../services/authService';

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    // 檢查本地存儲中的令牌
    const token = localStorage.getItem('token');
    const refreshTokenValue = localStorage.getItem('refreshToken');
    
    if (token) {
      // 驗證令牌或刷新令牌
      validateToken(token, refreshTokenValue);
    } else {
      setLoading(false);
    }
  }, []);

  const validateToken = async (token, refreshTokenValue) => {
    try {
      // 這裡可以添加令牌驗證邏輯
      // 如果令牌過期，嘗試使用刷新令牌
      if (isTokenExpired(token) && refreshTokenValue) {
        const response = await refreshToken(refreshTokenValue);
        localStorage.setItem('token', response.token);
        localStorage.setItem('refreshToken', response.refreshToken);
      }
      
      // 設置用戶信息
      setUser(parseUserFromToken(token));
    } catch (error) {
      // 如果刷新失敗，清除令牌
      logout();
      setError('Session expired. Please login again.');
    } finally {
      setLoading(false);
    }
  };

  const login = (userData, token, refreshTokenValue) => {
    localStorage.setItem('token', token);
    localStorage.setItem('refreshToken', refreshTokenValue);
    setUser(userData);
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    setUser(null);
  };

  const isTokenExpired = (token) => {
    // 令牌過期檢查邏輯
    // ...
    return false;
  };

  const parseUserFromToken = (token) => {
    // 從令牌中解析用戶信息
    // ...
    return { id: '1', name: 'User' };
  };

  return (
    <AuthContext.Provider value={{ user, login, logout, loading, error }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);
```

### 3. 路由設置

使用 React Router 進行路由管理：

```jsx
// App.js
import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { CartProvider } from './contexts/CartContext';
import Header from './components/common/Header';
import Footer from './components/common/Footer';
import Login from './pages/Login';
import Register from './pages/Register';
import ProductList from './pages/ProductList';
import ProductDetail from './pages/ProductDetail';
import Cart from './pages/Cart';
import Checkout from './pages/Checkout';
import NotFound from './pages/NotFound';

// 受保護的路由
const ProtectedRoute = ({ children }) => {
  const { user, loading } = useAuth();
  
  if (loading) return <div>Loading...</div>;
  
  if (!user) {
    return <Navigate to="/login" replace />;
  }
  
  return children;
};

const App = () => {
  return (
    <Router>
      <AuthProvider>
        <CartProvider>
          <div className="app-container">
            <Header />
            <main className="main-content">
              <Routes>
                <Route path="/" element={<ProductList />} />
                <Route path="/login" element={<Login />} />
                <Route path="/register" element={<Register />} />
                <Route path="/products" element={<ProductList />} />
                <Route path="/products/:id" element={<ProductDetail />} />
                <Route path="/cart" element={<Cart />} />
                <Route 
                  path="/checkout" 
                  element={
                    <ProtectedRoute>
                      <Checkout />
                    </ProtectedRoute>
                  } 
                />
                <Route path="*" element={<NotFound />} />
              </Routes>
            </main>
            <Footer />
          </div>
        </CartProvider>
      </AuthProvider>
    </Router>
  );
};

export default App;
```

## 五、響應式設計與跨裝置支援

為確保良好的用戶體驗，建議實現響應式設計：

1. **使用媒體查詢**：
   ```css
   /* 移動設備 */
   @media (max-width: 576px) {
     .product-grid {
       grid-template-columns: 1fr;
     }
   }
   
   /* 平板設備 */
   @media (min-width: 577px) and (max-width: 992px) {
     .product-grid {
       grid-template-columns: 1fr 1fr;
     }
   }
   
   /* 桌面設備 */
   @media (min-width: 993px) {
     .product-grid {
       grid-template-columns: 1fr 1fr 1fr 1fr;
     }
   }
   ```

2. **使用 Flexbox 和 Grid 佈局**：
   ```css
   .product-list {
     display: grid;
     grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
     gap: 20px;
   }
   
   .cart-summary {
     display: flex;
     flex-direction: column;
   }
   
   @media (min-width: 768px) {
     .cart-summary {
       flex-direction: row;
       justify-content: space-between;
     }
   }
   ```

## 結論

以上設計和實現方案提供了一個現代化電商平台的前端基礎架構。這些頁面設計考慮了用戶體驗、響應式設計和與後端API的整合。實際實現時，您可以根據具體需求和品牌風格進行調整和擴展。

關鍵建議：
1. 採用組件化開發，提高代碼復用性
2. 實現響應式設計，確保跨設備良好體驗
3. 使用狀態管理解決方案，處理複雜的應用狀態
4. 優化頁面加載性能，提供漸進式加載體驗
5. 實現錯誤處理和友好的用戶提示

這些設計和實現方案可以作為您開發電商平台前端的起點，幫助您快速構建功能完善、用戶友好的電商網站。