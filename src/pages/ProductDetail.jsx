import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { Row, Col, Carousel, Button, InputNumber, Tabs, Rate, Comment, Avatar, List, Spin, message, Breadcrumb, Tag } from 'antd';
import { ShoppingCartOutlined, HeartOutlined, ShareAltOutlined, CheckCircleOutlined, HomeOutlined, LeftOutlined, RightOutlined } from '@ant-design/icons';
import './ProductDetail.css';

const { TabPane } = Tabs;

/**
 * 商品詳情頁面元件
 * 
 * 展示商品詳細資訊、規格、評價和相關商品
 * 
 * @returns {JSX.Element} 商品詳情頁面
 */
const ProductDetail = () => {
  // 從 URL 獲取商品 ID
  const { id } = useParams();
  // 商品資訊
  const [product, setProduct] = useState(null);
  // 商品評價
  const [reviews, setReviews] = useState([]);
  // 相關商品
  const [relatedProducts, setRelatedProducts] = useState([]);
  // 載入狀態
  const [loading, setLoading] = useState(true);
  // 選擇的數量
  const [quantity, setQuantity] = useState(1);
  // 選擇的商品變體
  const [selectedVariant, setSelectedVariant] = useState(null);
  // 當前圖片索引
  const [currentImageIndex, setCurrentImageIndex] = useState(0);
  
  /**
   * 元件載入時獲取商品詳情
   */
  useEffect(() => {
    fetchProductDetails();
  }, [id]);
  
  /**
   * 從 API 獲取商品詳情
   */
  const fetchProductDetails = async () => {
    try {
      setLoading(true);
      
      // 模擬 API 呼叫
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      // 模擬商品資料
      const mockProduct = {
        id,
        name: `高品質商品 ${id}`,
        price: 3999,
        discountPrice: 3599,
        sku: `SKU-${id}-2023`,
        category: '電子產品',
        tags: ['熱銷', '推薦', '新品'],
        rating: 4.7,
        reviewCount: 128,
        stock: 25,
        description: `
          <p>這是一款高品質的商品，採用頂級材料製作而成。</p>
          <p>產品特點：</p>
          <ul>
            <li>高品質材料，耐用持久</li>
            <li>精美設計，美觀大方</li>
            <li>多種功能，滿足各種需求</li>
            <li>使用便捷，操作簡單</li>
          </ul>
          <p>適用場景：家庭、辦公室、戶外等多種場景</p>
        `,
        specifications: {
          '尺寸': '10 x 15 x 5 cm',
          '重量': '300g',
          '材質': '高級塑料 + 金屬',
          '顏色': '黑色/白色/藍色',
          '保固期': '1年',
          '產地': '台灣',
          '包裝內容': '主產品 x 1, 使用手冊 x 1, 配件 x 3'
        },
        images: [
          `https://picsum.photos/800/800?random=${id}-1`,
          `https://picsum.photos/800/800?random=${id}-2`,
          `https://picsum.photos/800/800?random=${id}-3`,
          `https://picsum.photos/800/800?random=${id}-4`
        ],
        variants: [
          { id: 1, name: '標準版', price: 3599, stock: 25 },
          { id: 2, name: '豪華版', price: 4599, stock: 10 },
          { id: 3, name: '限量版', price: 5599, stock: 5 }
        ]
      };
      
      setProduct(mockProduct);
      
      // 如果有變體，默認選擇第一個
      if (mockProduct.variants && mockProduct.variants.length > 0) {
        setSelectedVariant(mockProduct.variants[0]);
      }
      
      // 模擬評論資料
      const mockReviews = Array(5).fill().map((_, index) => ({
        id: index + 1,
        userName: `用戶${index + 1}`,
        userAvatar: `https://randomuser.me/api/portraits/men/${index + 20}.jpg`,
        rating: Math.floor(Math.random() * 2) + 4, // 4-5 星評價
        content: `這是一個很棒的產品！我非常滿意這次的購買。商品品質優良，包裝完整，運送迅速。${index % 2 === 0 ? '會推薦給朋友購買。' : ''}`,
        createdAt: new Date(Date.now() - (index * 86400000)).toISOString() // 每天一條評論
      }));
      
      setReviews(mockReviews);
      
      // 模擬相關商品
      const mockRelatedProducts = Array(6).fill().map((_, index) => ({
        id: Number(id) + index + 1,
        name: `相關商品 ${Number(id) + index + 1}`,
        price: Math.floor(Math.random() * 3000) + 2000,
        imageUrl: `https://picsum.photos/300/300?random=${Number(id) + index + 1}`
      }));
      
      setRelatedProducts(mockRelatedProducts);
    } catch (error) {
      console.error('獲取商品詳情失敗:', error);
      message.error('無法載入商品詳情，請稍後再試');
    } finally {
      setLoading(false);
    }
  };
  
  /**
   * 處理加入購物車
   */
  const handleAddToCart = async () => {
    try {
      // 模擬 API 呼叫
      await new Promise(resolve => setTimeout(resolve, 500));
      
      message.success('商品已加入購物車！');
    } catch (error) {
      message.error('加入購物車失敗，請稍後再試');
    }
  };
  
  /**
   * 處理數量變更
   * 
   * @param {number} value - 選擇的數量
   */
  const handleQuantityChange = (value) => {
    setQuantity(value);
  };
  
  /**
   * 處理變體選擇
   * 
   * @param {Object} variant - 選擇的變體
   */
  const handleVariantSelect = (variant) => {
    setSelectedVariant(variant);
  };

  /**
   * 處理圖片輪播切換
   * 
   * @param {number} index - 圖片索引
   */
  const handleImageChange = (index) => {
    setCurrentImageIndex(index);
  };

  /**
   * 處理加入收藏
   */
  const handleAddToWishlist = () => {
    message.success('商品已加入收藏！');
  };

  /**
   * 處理分享商品
   */
  const handleShare = () => {
    // 實際應用中會實現分享功能
    message.info('分享功能開發中');
  };
  
  /**
   * 計算實際價格
   * 
   * @returns {number} 實際價格
   */
  const calculateActualPrice = () => {
    if (selectedVariant) {
      return selectedVariant.price;
    }
    return product.discountPrice || product.price;
  };

  /**
   * 計算折扣百分比
   * 
   * @returns {number} 折扣百分比
   */
  const calculateDiscountPercentage = () => {
    if (product.discountPrice && product.price) {
      return Math.round((1 - product.discountPrice / product.price) * 100);
    }
    return 0;
  };
  
  if (loading) {
    return (
      <div className="loading-container">
        <Spin size="large" />
      </div>
    );
  }
  
  if (!product) {
    return <div className="product-not-found">商品不存在或已被移除</div>;
  }
  
  return (
    <div className="product-detail-container">
      <Breadcrumb className="product-breadcrumb">
        <Breadcrumb.Item>
          <Link to="/"><HomeOutlined /> 首頁</Link>
        </Breadcrumb.Item>
        <Breadcrumb.Item>
          <Link to="/products">所有商品</Link>
        </Breadcrumb.Item>
        <Breadcrumb.Item>
          <Link to={`/category/${product.category}`}>{product.category}</Link>
        </Breadcrumb.Item>
        <Breadcrumb.Item>{product.name}</Breadcrumb.Item>
      </Breadcrumb>
      
      <Row gutter={[32, 32]} className="product-main-info">
        <Col xs={24} md={12}>
          <div className="product-gallery">
            <div className="product-main-image">
              <img src={product.images[currentImageIndex]} alt={`${product.name} - ${currentImageIndex + 1}`} />
              <div className="image-navigation">
                <Button 
                  className="nav-button prev" 
                  icon={<LeftOutlined />}
                  disabled={currentImageIndex === 0}
                  onClick={() => setCurrentImageIndex(prev => Math.max(0, prev - 1))}
                />
                <Button 
                  className="nav-button next" 
                  icon={<RightOutlined />}
                  disabled={currentImageIndex === product.images.length - 1}
                  onClick={() => setCurrentImageIndex(prev => Math.min(product.images.length - 1, prev + 1))}
                />
              </div>
            </div>
            
            <div className="product-thumbnails">
              {product.images.map((image, index) => (
                <div 
                  key={index} 
                  className={`thumbnail ${currentImageIndex === index ? 'active' : ''}`}
                  onClick={() => handleImageChange(index)}
                >
                  <img src={image} alt={`thumbnail-${index}`} />
                </div>
              ))}
            </div>
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
                  <span className="discount-tag">節省 {calculateDiscountPercentage()}%</span>
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
                      <div className="variant-name">{variant.name}</div>
                      <div className="variant-price">NT$ {variant.price.toLocaleString()}</div>
                      {variant.stock <= 5 && <div className="variant-stock">僅剩 {variant.stock} 件</div>}
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
                  max={selectedVariant?.stock || product.stock} 
                  defaultValue={1} 
                  onChange={handleQuantityChange} 
                />
              </div>
              
              <div className="stock-info">
                {(selectedVariant?.stock || product.stock) > 0 ? (
                  <span className="in-stock">
                    <CheckCircleOutlined /> 有庫存 ({selectedVariant?.stock || product.stock})
                  </span>
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
                  disabled={(selectedVariant?.stock || product.stock) === 0}
                  className="add-to-cart-btn"
                >
                  加入購物車
                </Button>
                
                <Button 
                  icon={<HeartOutlined />} 
                  size="large"
                  onClick={handleAddToWishlist}
                  className="wishlist-btn"
                >
                  加入收藏
                </Button>
                
                <Button 
                  icon={<ShareAltOutlined />} 
                  size="large"
                  onClick={handleShare}
                  className="share-btn"
                >
                  分享
                </Button>
              </div>
            </div>
            
            <div className="product-meta">
              <p><strong>商品編號：</strong> {product.sku}</p>
              <p><strong>類別：</strong> {product.category}</p>
              <p>
                <strong>標籤：</strong> 
                {product.tags.map(tag => (
                  <Tag key={tag}>{tag}</Tag>
                ))}
              </p>
            </div>
          </div>
        </Col>
      </Row>
      
      <div className="product-details-tabs">
        <Tabs defaultActiveKey="description">
          <TabPane tab="商品描述" key="description">
            <div className="product-description" dangerouslySetInnerHTML={{ __html: product.description }} />
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
                {[5, 4, 3, 2, 1].map(star => (
                  <div key={star} className="rating-bar">
                    <span className="star-label">{star} 星</span>
                    <div className="progress-bar">
                      <div 
                        className="progress" 
                        style={{ 
                          width: `${Math.floor(Math.random() * 50) + (star === 5 ? 50 : star === 4 ? 30 : 10)}%` 
                        }}
                      ></div>
                    </div>
                    <span className="percentage">
                      {Math.floor(Math.random() * 50) + (star === 5 ? 50 : star === 4 ? 30 : 10)}%
                    </span>
                  </div>
                ))}
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
              <Link to={`/products/${item.id}`} className="related-product-link">
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