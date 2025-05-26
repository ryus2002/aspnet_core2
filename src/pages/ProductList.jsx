import React, { useState, useEffect } from 'react';
import { Row, Col, Card, Input, Select, Pagination, Slider, Tag, Spin, Empty, Button } from 'antd';
import { ShoppingCartOutlined, HeartOutlined, FilterOutlined, AppstoreOutlined, BarsOutlined } from '@ant-design/icons';
import { Link } from 'react-router-dom';
import './ProductList.css';

const { Search } = Input;
const { Option } = Select;
const { Meta } = Card;

/**
 * 商品列表頁面元件
 * 
 * 提供商品瀏覽、搜尋、篩選和排序功能
 * 
 * @returns {JSX.Element} 商品列表頁面
 */
const ProductList = () => {
  // 商品資料狀態
  const [products, setProducts] = useState([]);
  // 載入狀態
  const [loading, setLoading] = useState(true);
  // 視圖模式：網格或列表
  const [viewMode, setViewMode] = useState('grid');
  // 篩選條件
  const [filters, setFilters] = useState({
    category: 'all',
    priceRange: [0, 10000],
    sortBy: 'newest'
  });
  // 分頁資訊
  const [pagination, setPagination] = useState({
    current: 1,
    pageSize: 12,
    total: 0
  });

  /**
   * 元件載入時獲取商品資料
   */
  useEffect(() => {
    fetchProducts();
  }, [filters, pagination.current]);

  /**
   * 從 API 獲取商品資料
   */
  const fetchProducts = async () => {
    try {
      setLoading(true);
      
      // 模擬 API 呼叫
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      // 模擬商品資料
      const mockProducts = Array(20).fill().map((_, index) => ({
        id: index + 1,
        name: `商品 ${index + 1}`,
        price: Math.floor(Math.random() * 9000) + 1000,
        discount: index % 3 === 0 ? Math.floor(Math.random() * 30) + 10 : 0,
        imageUrl: `https://picsum.photos/300/300?random=${index}`,
        category: ['電子產品', '服飾', '家居'][index % 3],
        rating: (Math.random() * 3 + 2).toFixed(1),
        reviewCount: Math.floor(Math.random() * 100) + 5,
        inStock: Math.random() > 0.2
      }));
      
      // 根據篩選條件過濾
      let filteredProducts = [...mockProducts];
      
      // 類別篩選
      if (filters.category !== 'all') {
        const categoryMap = {
          'electronics': '電子產品',
          'clothing': '服飾',
          'home': '家居'
        };
        filteredProducts = filteredProducts.filter(p => 
          p.category === categoryMap[filters.category]
        );
      }
      
      // 價格範圍篩選
      filteredProducts = filteredProducts.filter(p => 
        p.price >= filters.priceRange[0] && p.price <= filters.priceRange[1]
      );
      
      // 排序
      switch (filters.sortBy) {
        case 'price-asc':
          filteredProducts.sort((a, b) => a.price - b.price);
          break;
        case 'price-desc':
          filteredProducts.sort((a, b) => b.price - a.price);
          break;
        case 'popular':
          filteredProducts.sort((a, b) => b.reviewCount - a.reviewCount);
          break;
        case 'newest':
        default:
          // 假設已經按最新排序
          break;
      }
      
      // 分頁
      const startIndex = (pagination.current - 1) * pagination.pageSize;
      const paginatedProducts = filteredProducts.slice(
        startIndex, 
        startIndex + pagination.pageSize
      );
      
      setProducts(paginatedProducts);
      setPagination({
        ...pagination,
        total: filteredProducts.length
      });
    } catch (error) {
      console.error('獲取商品失敗:', error);
    } finally {
      setLoading(false);
    }
  };

  /**
   * 處理類別變更
   * 
   * @param {string} value - 選擇的類別
   */
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

  /**
   * 處理排序方式變更
   * 
   * @param {string} value - 選擇的排序方式
   */
  const handleSortChange = (value) => {
    setFilters({
      ...filters,
      sortBy: value
    });
  };

  /**
   * 處理價格範圍變更
   * 
   * @param {Array} value - 價格範圍 [最小值, 最大值]
   */
  const handlePriceRangeChange = (value) => {
    setFilters({
      ...filters,
      priceRange: value
    });
  };

  /**
   * 處理搜尋
   * 
   * @param {string} value - 搜尋關鍵字
   */
  const handleSearch = (value) => {
    // 實際應用中會將搜尋關鍵字傳遞給 API
    console.log('搜尋:', value);
    setPagination({
      ...pagination,
      current: 1
    });
  };

  /**
   * 處理頁碼變更
   * 
   * @param {number} page - 頁碼
   */
  const handlePageChange = (page) => {
    setPagination({
      ...pagination,
      current: page
    });
  };

  /**
   * 處理加入購物車
   * 
   * @param {Object} product - 商品資訊
   * @param {Event} e - 事件對象
   */
  const handleAddToCart = (product, e) => {
    e.preventDefault();
    e.stopPropagation();
    console.log('加入購物車:', product);
    // 實際應用中會調用購物車服務
  };

  /**
   * 處理加入收藏
   * 
   * @param {Object} product - 商品資訊
   * @param {Event} e - 事件對象
   */
  const handleAddToWishlist = (product, e) => {
    e.preventDefault();
    e.stopPropagation();
    console.log('加入收藏:', product);
    // 實際應用中會調用收藏服務
  };

  /**
   * 渲染商品網格
   * 
   * @returns {JSX.Element} 商品網格元件
   */
  const renderProductGrid = () => {
    if (products.length === 0 && !loading) {
      return <Empty description="沒有找到符合條件的商品" />;
    }

    return (
      <Row gutter={[16, 24]} className="product-grid">
        {products.map(product => (
          <Col xs={24} sm={12} md={8} lg={6} key={product.id}>
            <Card
              hoverable
              className="product-card"
              cover={
                <div className="product-image-container">
                  <img alt={product.name} src={product.imageUrl} className="product-image" />
                  {!product.inStock && <div className="out-of-stock-overlay">缺貨中</div>}
                </div>
              }
              actions={[
                <Button 
                  icon={<HeartOutlined />} 
                  onClick={(e) => handleAddToWishlist(product, e)}
                  type="text"
                >
                  收藏
                </Button>,
                <Button 
                  icon={<ShoppingCartOutlined />} 
                  onClick={(e) => handleAddToCart(product, e)}
                  type="primary"
                  disabled={!product.inStock}
                >
                  加入購物車
                </Button>
              ]}
            >
              <Link to={`/products/${product.id}`} className="product-link">
                <Meta 
                  title={<span className="product-name">{product.name}</span>} 
                  description={
                    <div className="product-info">
                      <div className="product-price-container">
                        {product.discount > 0 ? (
                          <>
                            <span className="product-original-price">NT$ {product.price.toLocaleString()}</span>
                            <span className="product-discount-price">
                              NT$ {Math.round(product.price * (1 - product.discount / 100)).toLocaleString()}
                            </span>
                          </>
                        ) : (
                          <span className="product-price">NT$ {product.price.toLocaleString()}</span>
                        )}
                      </div>
                      <div className="product-meta">
                        {product.discount > 0 && (
                          <Tag color="red" className="discount-tag">節省 {product.discount}%</Tag>
                        )}
                        <div className="product-rating">
                          <span>★</span> {product.rating} ({product.reviewCount})
                        </div>
                      </div>
                    </div>
                  }
                />
              </Link>
            </Card>
          </Col>
        ))}
      </Row>
    );
  };

  /**
   * 渲染商品列表
   * 
   * @returns {JSX.Element} 商品列表元件
   */
  const renderProductList = () => {
    if (products.length === 0 && !loading) {
      return <Empty description="沒有找到符合條件的商品" />;
    }

    return (
      <div className="product-list-view">
        {products.map(product => (
          <Link to={`/products/${product.id}`} key={product.id} className="product-list-item">
            <div className="product-list-image">
              <img alt={product.name} src={product.imageUrl} />
              {!product.inStock && <div className="out-of-stock-overlay">缺貨中</div>}
            </div>
            <div className="product-list-content">
              <h3 className="product-list-name">{product.name}</h3>
              <div className="product-list-meta">
                <div className="product-list-rating">
                  <span>★</span> {product.rating} ({product.reviewCount})
                </div>
                <div className="product-list-category">{product.category}</div>
              </div>
              <div className="product-list-price-container">
                {product.discount > 0 ? (
                  <>
                    <span className="product-list-original-price">NT$ {product.price.toLocaleString()}</span>
                    <span className="product-list-discount-price">
                      NT$ {Math.round(product.price * (1 - product.discount / 100)).toLocaleString()}
                    </span>
                    <Tag color="red" className="discount-tag">節省 {product.discount}%</Tag>
                  </>
                ) : (
                  <span className="product-list-price">NT$ {product.price.toLocaleString()}</span>
                )}
              </div>
            </div>
            <div className="product-list-actions">
              <Button 
                icon={<HeartOutlined />} 
                onClick={(e) => handleAddToWishlist(product, e)}
                className="product-list-wishlist-btn"
              >
                收藏
              </Button>
              <Button 
                icon={<ShoppingCartOutlined />} 
                onClick={(e) => handleAddToCart(product, e)}
                type="primary"
                disabled={!product.inStock}
                className="product-list-cart-btn"
              >
                加入購物車
              </Button>
            </div>
          </Link>
        ))}
      </div>
    );
  };

  return (
    <div className="product-list-container">
      <div className="product-list-header">
        <h1 className="page-title">所有商品</h1>
        <div className="product-list-tools">
          <Search
            placeholder="搜尋商品"
            allowClear
            onSearch={handleSearch}
            className="product-search"
          />
          
          <Select 
            defaultValue="all" 
            className="category-select" 
            onChange={handleCategoryChange}
          >
            <Option value="all">所有類別</Option>
            <Option value="electronics">電子產品</Option>
            <Option value="clothing">服飾</Option>
            <Option value="home">家居</Option>
          </Select>
          
          <Select
            defaultValue="newest"
            className="sort-select"
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
              className="price-slider"
            />
            <div className="price-range-display">
              NT$ {filters.priceRange[0].toLocaleString()} - NT$ {filters.priceRange[1].toLocaleString()}
            </div>
          </div>
          
          {/* 其他篩選選項可在此添加 */}
          <div className="filter-section">
            <h3>商品狀態</h3>
            <div className="filter-options">
              <label className="filter-checkbox">
                <input type="checkbox" defaultChecked /> 有庫存
              </label>
              <label className="filter-checkbox">
                <input type="checkbox" defaultChecked /> 有折扣
              </label>
            </div>
          </div>
          
          <div className="filter-section">
            <h3>評分</h3>
            <div className="filter-options">
              {[5, 4, 3, 2, 1].map(rating => (
                <label key={rating} className="filter-checkbox">
                  <input type="checkbox" defaultChecked={rating >= 3} />
                  {rating} 星以上
                </label>
              ))}
            </div>
          </div>
        </div>
        
        <div className="product-list-main">
          {loading ? (
            <div className="loading-container">
              <Spin size="large" />
            </div>
          ) : (
            viewMode === 'grid' ? renderProductGrid() : renderProductList()
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