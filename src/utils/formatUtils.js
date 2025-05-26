/**
 * 格式化工具函數
 * 提供日期、貨幣等格式化功能
 */

/**
 * 格式化日期
 * @param {string|Date} dateString - 日期字符串或日期對象
 * @param {boolean} showTime - 是否顯示時間
 * @returns {string} 格式化後的日期字符串
 */
export const formatDate = (dateString, showTime = false) => {
  if (!dateString) return '-';
  
  try {
    const date = new Date(dateString);
    
    // 檢查日期是否有效
    if (isNaN(date.getTime())) {
      return '-';
    }
    
    // 日期格式化選項
    const options = {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
    };
    
    // 如果需要顯示時間，添加時間格式化選項
    if (showTime) {
      options.hour = '2-digit';
      options.minute = '2-digit';
      options.second = '2-digit';
      options.hour12 = false;
    }
    
    // 使用 Intl.DateTimeFormat 進行本地化格式化
    return new Intl.DateTimeFormat('zh-TW', options).format(date);
  } catch (error) {
    console.error('日期格式化錯誤:', error);
    return dateString || '-';
  }
};

/**
 * 格式化貨幣
 * @param {number} amount - 金額
 * @param {string} currency - 貨幣代碼，默認為 TWD
 * @returns {string} 格式化後的貨幣字符串
 */
export const formatCurrency = (amount, currency = 'TWD') => {
  if (amount === null || amount === undefined) return '-';
  
  try {
    // 將金額轉換為數字
    const numericAmount = Number(amount);
    
    // 檢查金額是否為有效數字
    if (isNaN(numericAmount)) {
      return '-';
    }
    
    // 使用 Intl.NumberFormat 進行本地化貨幣格式化
    return new Intl.NumberFormat('zh-TW', {
      style: 'currency',
      currency: currency,
      minimumFractionDigits: 0,
      maximumFractionDigits: 2
    }).format(numericAmount);
  } catch (error) {
    console.error('貨幣格式化錯誤:', error);
    return `${amount} ${currency}` || '-';
  }
};

/**
 * 格式化電話號碼
 * @param {string} phoneNumber - 電話號碼
 * @returns {string} 格式化後的電話號碼
 */
export const formatPhoneNumber = (phoneNumber) => {
  if (!phoneNumber) return '-';
  
  try {
    // 移除所有非數字字符
    const cleaned = phoneNumber.replace(/\D/g, '');
    
    // 根據長度格式化電話號碼
    if (cleaned.length === 10) {
      // 例如: 0912345678 -> 0912-345-678
      return cleaned.replace(/(\d{4})(\d{3})(\d{3})/, '$1-$2-$3');
    } else if (cleaned.length === 9) {
      // 例如: 912345678 -> 912-345-678
      return cleaned.replace(/(\d{3})(\d{3})(\d{3})/, '$1-$2-$3');
    } else if (cleaned.length === 8) {
      // 例如: 12345678 -> 1234-5678
      return cleaned.replace(/(\d{4})(\d{4})/, '$1-$2');
    } else {
      // 其他長度不做特殊格式化
      return phoneNumber;
    }
  } catch (error) {
    console.error('電話號碼格式化錯誤:', error);
    return phoneNumber || '-';
  }
};

/**
 * 截斷文本並添加省略號
 * @param {string} text - 原始文本
 * @param {number} maxLength - 最大長度
 * @returns {string} 截斷後的文本
 */
export const truncateText = (text, maxLength = 50) => {
  if (!text) return '';
  
  if (text.length <= maxLength) {
    return text;
  }
  
  return text.substring(0, maxLength) + '...';
};