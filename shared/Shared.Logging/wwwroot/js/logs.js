/**
 * 日誌查看工具的前端 JavaScript
 */

// API 基礎路徑
const API_BASE_URL = '/api/logs';

// 當前頁碼和每頁記錄數
let currentPage = 1;
const pageSize = 20;

// 圖表實例
let levelChart = null;
let serviceChart = null;
let timeChart = null;

// 頁面載入完成後執行
document.addEventListener('DOMContentLoaded', function() {
    // 初始化事件處理
    initEventHandlers();
    
    // 載入日誌文件列表
    loadLogFiles();
    
    // 設置默認時間範圍（過去 24 小時）
    setDefaultTimeRange();
});

/**
 * 初始化事件處理
 */
function initEventHandlers() {
    // 搜索表單提交
    document.getElementById('searchForm').addEventListener('submit', function(e) {
        e.preventDefault();
        currentPage = 1;
        searchLogs();
    });
    
    // 統計表單提交
    document.getElementById('statisticsForm').addEventListener('submit', function(e) {
        e.preventDefault();
        loadStatistics();
    });
    
    // 刷新文件列表按鈕
    document.getElementById('refreshFiles').addEventListener('click', function() {
        loadLogFiles();
    });
    
    // 分頁按鈕
    document.getElementById('prevPage').addEventListener('click', function() {
        if (currentPage > 1) {
            currentPage--;
            searchLogs();
        }
    });
    
    document.getElementById('nextPage').addEventListener('click', function() {
        currentPage++;
        searchLogs();
    });
    
    // 標籤頁切換事件
    document.getElementById('statistics-tab').addEventListener('click', function() {
        // 延遲加載統計數據，以確保圖表容器已經顯示
        setTimeout(function() {
            if (!levelChart && !serviceChart && !timeChart) {
                loadStatistics();
            }
        }, 100);
    });
}

/**
 * 設置默認時間範圍（過去 24 小時）
 */
function setDefaultTimeRange() {
    const now = new Date();
    const yesterday = new Date(now);
    yesterday.setDate(yesterday.getDate() - 1);
    
    const formatDateTime = (date) => {
        return date.toISOString().slice(0, 16);
    };
    
    document.getElementById('startTime').value = formatDateTime(yesterday);
    document.getElementById('endTime').value = formatDateTime(now);
    document.getElementById('statsStartTime').value = formatDateTime(yesterday);
    document.getElementById('statsEndTime').value = formatDateTime(now);
}

/**
 * 載入日誌文件列表
 */
function loadLogFiles() {
    const serviceName = document.getElementById('fileServiceName').value.trim();
    const filesList = document.getElementById('filesList');
    
    filesList.innerHTML = '<div class="text-center text-muted">載入中...</div>';
    
    fetch(`${API_BASE_URL}/files${serviceName ? `?serviceName=${encodeURIComponent(serviceName)}` : ''}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('載入日誌文件列表失敗');
            }
            return response.json();
        })
        .then(files => {
            if (files.length === 0) {
                filesList.innerHTML = '<div class="text-center text-muted">沒有找到日誌文件</div>';
                return;
            }
            
            filesList.innerHTML = '';
            files.forEach(file => {
                const fileName = file.split('/').pop();
                const listItem = document.createElement('a');
                listItem.href = '#';
                listItem.className = 'list-group-item list-group-item-action';
                listItem.textContent = fileName;
                listItem.title = file;
                listItem.addEventListener('click', function(e) {
                    e.preventDefault();
                    loadFileContent(file);
                    
                    // 移除其他項目的活動狀態
                    document.querySelectorAll('#filesList .active').forEach(item => {
                        item.classList.remove('active');
                    });
                    
                    // 添加當前項目的活動狀態
                    this.classList.add('active');
                });
                
                filesList.appendChild(listItem);
            });
        })
        .catch(error => {
            console.error('載入日誌文件列表時發生錯誤:', error);
            filesList.innerHTML = `<div class="text-center text-danger">載入失敗: ${error.message}</div>`;
        });
}

/**
 * 載入文件內容
 * @param {string} filePath - 文件路徑
 */
function loadFileContent(filePath) {
    const fileContent = document.getElementById('fileContent');
    
    fileContent.innerHTML = '<div class="text-center text-muted">載入中...</div>';
    
    fetch(`${API_BASE_URL}/file?filePath=${encodeURIComponent(filePath)}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('載入文件內容失敗');
            }
            return response.json();
        })
        .then(events => {
            if (events.length === 0) {
                fileContent.innerHTML = '<div class="text-center text-muted">文件為空</div>';
                return;
            }
            
            fileContent.innerHTML = '';
            events.forEach(event => {
                fileContent.appendChild(createLogEventElement(event));
            });
        })
        .catch(error => {
            console.error('載入文件內容時發生錯誤:', error);
            fileContent.innerHTML = `<div class="text-center text-danger">載入失敗: ${error.message}</div>`;
        });
}

/**
 * 搜索日誌
 */
function searchLogs() {
    const searchResults = document.getElementById('searchResults');
    const prevPageBtn = document.getElementById('prevPage');
    const nextPageBtn = document.getElementById('nextPage');
    const pageInfo = document.getElementById('pageInfo');
    
    // 獲取搜索參數
    const serviceName = document.getElementById('serviceName').value.trim();
    const level = document.getElementById('level').value;
    const searchText = document.getElementById('searchText').value.trim();
    const startTime = document.getElementById('startTime').value;
    const endTime = document.getElementById('endTime').value;
    
    // 構建查詢參數
    const params = new URLSearchParams();
    if (serviceName) params.append('serviceName', serviceName);
    if (level) params.append('level', level);
    if (searchText) params.append('searchText', searchText);
    if (startTime) params.append('startTime', new Date(startTime).toISOString());
    if (endTime) params.append('endTime', new Date(endTime).toISOString());
    params.append('skip', (currentPage - 1) * pageSize);
    params.append('take', pageSize);
    
    // 顯示載入中
    searchResults.innerHTML = '<div class="text-center text-muted">搜索中...</div>';
    
    // 發送請求
    fetch(`${API_BASE_URL}/search?${params.toString()}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('搜索日誌失敗');
            }
            return response.json();
        })
        .then(events => {
            if (events.length === 0) {
                searchResults.innerHTML = '<div class="text-center text-muted">沒有找到匹配的日誌記錄</div>';
                nextPageBtn.disabled = true;
            } else {
                searchResults.innerHTML = '';
                events.forEach(event => {
                    searchResults.appendChild(createLogEventElement(event));
                });
                
                // 如果返回的記錄數少於每頁大小，禁用下一頁按鈕
                nextPageBtn.disabled = events.length < pageSize;
            }
            
            // 更新分頁信息
            prevPageBtn.disabled = currentPage <= 1;
            pageInfo.textContent = `第 ${currentPage} 頁`;
        })
        .catch(error => {
            console.error('搜索日誌時發生錯誤:', error);
            searchResults.innerHTML = `<div class="text-center text-danger">搜索失敗: ${error.message}</div>`;
        });
}

/**
 * 載入統計數據
 */
function loadStatistics() {
    // 獲取統計參數
    const serviceName = document.getElementById('statsServiceName').value.trim();
    const startTime = document.getElementById('statsStartTime').value;
    const endTime = document.getElementById('statsEndTime').value;
    
    // 構建查詢參數
    const params = new URLSearchParams();
    if (serviceName) params.append('serviceName', serviceName);
    if (startTime) params.append('startTime', new Date(startTime).toISOString());
    if (endTime) params.append('endTime', new Date(endTime).toISOString());
    
    // 發送請求
    fetch(`${API_BASE_URL}/statistics?${params.toString()}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('載入統計數據失敗');
            }
            return response.json();
        })
        .then(statistics => {
            // 更新圖表
            updateLevelChart(statistics.countByLevel);
            updateServiceChart(statistics.countByService);
            updateTimeChart(statistics.countByHour);
            
            // 更新錯誤列表
            updateCommonErrors(statistics.mostCommonErrors);
            
            // 更新最慢請求列表
            updateSlowestRequests(statistics.slowestRequests);
        })
        .catch(error => {
            console.error('載入統計數據時發生錯誤:', error);
            alert(`載入統計數據失敗: ${error.message}`);
        });
}

/**
 * 更新日誌級別統計圖表
 * @param {Object} countByLevel - 按級別統計的記錄數
 */
function updateLevelChart(countByLevel) {
    const ctx = document.getElementById('levelChart').getContext('2d');
    
    // 如果圖表已存在，則銷毀它
    if (levelChart) {
        levelChart.destroy();
    }
    
    // 準備數據
    const labels = Object.keys(countByLevel);
    const data = Object.values(countByLevel);
    const backgroundColors = labels.map(level => {
        switch (level.toLowerCase()) {
            case 'verbose': return '#6c757d';
            case 'debug': return '#17a2b8';
            case 'information': return '#28a745';
            case 'warning': return '#ffc107';
            case 'error': return '#dc3545';
            case 'fatal': return '#7e0000';
            default: return '#007bff';
        }
    });
    
    // 創建圖表
    levelChart = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: backgroundColors
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'right'
                },
                title: {
                    display: true,
                    text: '按日誌級別統計'
                }
            }
        }
    });
}

/**
 * 更新服務統計圖表
 * @param {Object} countByService - 按服務統計的記錄數
 */
function updateServiceChart(countByService) {
    const ctx = document.getElementById('serviceChart').getContext('2d');
    
    // 如果圖表已存在，則銷毀它
    if (serviceChart) {
        serviceChart.destroy();
    }
    
    // 準備數據
    const labels = Object.keys(countByService);
    const data = Object.values(countByService);
    
    // 創建圖表
    serviceChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: '記錄數',
                data: data,
                backgroundColor: '#007bff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                title: {
                    display: true,
                    text: '按服務名稱統計'
                }
            },
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}

/**
 * 更新時間統計圖表
 * @param {Object} countByHour - 按小時統計的記錄數
 */
function updateTimeChart(countByHour) {
    const ctx = document.getElementById('timeChart').getContext('2d');
    
    // 如果圖表已存在，則銷毀它
    if (timeChart) {
        timeChart.destroy();
    }
    
    // 準備數據
    const labels = Object.keys(countByHour).sort();
    const data = labels.map(hour => countByHour[hour]);
    
    // 創建圖表
    timeChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: '記錄數',
                data: data,
                borderColor: '#007bff',
                backgroundColor: 'rgba(0, 123, 255, 0.1)',
                fill: true,
                tension: 0.1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                title: {
                    display: true,
                    text: '按時間統計'
                }
            },
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}

/**
 * 更新最常見錯誤列表
 * @param {Array} errors - 最常見的錯誤
 */
function updateCommonErrors(errors) {
    const container = document.getElementById('commonErrors');
    
    if (!errors || errors.length === 0) {
        container.innerHTML = '<li class="list-group-item text-center text-muted">無數據</li>';
        return;
    }
    
    container.innerHTML = '';
    errors.forEach(error => {
        const listItem = document.createElement('li');
        listItem.className = 'list-group-item d-flex justify-content-between align-items-center';
        
        const errorText = document.createElement('span');
        errorText.textContent = truncateText(error.key, 100);
        errorText.title = error.key;
        
        const badge = document.createElement('span');
        badge.className = 'badge bg-danger rounded-pill';
        badge.textContent = error.value;
        
        listItem.appendChild(errorText);
        listItem.appendChild(badge);
        container.appendChild(listItem);
    });
}

/**
 * 更新最慢請求列表
 * @param {Array} requests - 最慢的請求
 */
function updateSlowestRequests(requests) {
    const container = document.getElementById('slowestRequests');
    
    if (!requests || requests.length === 0) {
        container.innerHTML = '<li class="list-group-item text-center text-muted">無數據</li>';
        return;
    }
    
    container.innerHTML = '';
    requests.forEach(request => {
        const listItem = document.createElement('li');
        listItem.className = 'list-group-item d-flex justify-content-between align-items-center';
        
        const requestText = document.createElement('span');
        requestText.textContent = truncateText(request.renderedMessage, 100);
        requestText.title = request.renderedMessage;
        
        const badge = document.createElement('span');
        badge.className = 'badge bg-warning rounded-pill';
        const elapsedMs = request.properties.ElapsedMs;
        badge.textContent = `${elapsedMs}ms`;
        
        listItem.appendChild(requestText);
        listItem.appendChild(badge);
        container.appendChild(listItem);
    });
}

/**
 * 創建日誌事件元素
 * @param {Object} event - 日誌事件
 * @returns {HTMLElement} 日誌事件元素
 */
function createLogEventElement(event) {
    const listItem = document.createElement('a');
    listItem.href = '#';
    listItem.className = `list-group-item list-group-item-action log-level-${event.level.toLowerCase()}`;
    
    // 創建時間戳
    const timestamp = document.createElement('small');
    timestamp.className = 'text-muted me-2';
    timestamp.textContent = new Date(event.timestamp).toLocaleString();
    
    // 創建級別標籤
    const levelBadge = document.createElement('span');
    levelBadge.className = `badge me-2 log-level-${event.level.toLowerCase()}`;
    levelBadge.textContent = event.level;
    
    // 創建消息
    const message = document.createElement('span');
    message.textContent = truncateText(event.renderedMessage, 150);
    
    // 添加點擊事件
    listItem.addEventListener('click', function(e) {
        e.preventDefault();
        showLogDetail(event);
    });
    
    // 組合元素
    listItem.appendChild(timestamp);
    listItem.appendChild(levelBadge);
    listItem.appendChild(message);
    
    return listItem;
}

/**
 * 顯示日誌詳情
 * @param {Object} event - 日誌事件
 */
function showLogDetail(event) {
    const logDetail = document.getElementById('logDetail');
    const modal = new bootstrap.Modal(document.getElementById('logDetailModal'));
    
    // 創建詳情內容
    let content = `
        <div class="mb-3">
            <h6>時間</h6>
            <p>${new Date(event.timestamp).toLocaleString()}</p>
        </div>
        <div class="mb-3">
            <h6>級別</h6>
            <p><span class="badge log-level-${event.level.toLowerCase()}">${event.level}</span></p>
        </div>
        <div class="mb-3">
            <h6>消息</h6>
            <p>${event.renderedMessage}</p>
        </div>
    `;
    
    // 如果有異常信息
    if (event.exception) {
        content += `
            <div class="mb-3">
                <h6>異常</h6>
                <pre class="bg-light p-2 rounded">${event.exception}</pre>
            </div>
        `;
    }
    
    // 如果有屬性
    if (Object.keys(event.properties).length > 0) {
        content += `
            <div class="mb-3">
                <h6>屬性</h6>
                <pre class="bg-light p-2 rounded">${JSON.stringify(event.properties, null, 2)}</pre>
            </div>
        `;
    }
    
    // 設置內容並顯示模態框
    logDetail.innerHTML = content;
    modal.show();
}

/**
 * 截斷文本
 * @param {string} text - 原始文本
 * @param {number} maxLength - 最大長度
 * @returns {string} 截斷後的文本
 */
function truncateText(text, maxLength) {
    if (!text) return '';
    return text.length > maxLength ? text.substring(0, maxLength) + '...' : text;
}