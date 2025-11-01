(function (global) {
    'use strict';

    function normalizeBase(url) {
        if (!url) {
            return null;
        }

        try {
            return String(url).trim().replace(/\/$/, '');
        } catch (_) {
            return null;
        }
    }

    function detectBaseUrl() {
        if (typeof document !== 'undefined') {
            try {
                var meta = document.querySelector('meta[name="gateway-origin"], meta[name="feane-gateway-base-url"]');
                if (meta && normalizeBase(meta.content)) {
                    console.log('[GatewayClient] Using meta tag gateway URL:', meta.content);
                    return normalizeBase(meta.content);
                }

                var script = document.currentScript;
                if (script) {
                    var dataAttr = script.getAttribute('data-gateway-origin') || script.getAttribute('data-gateway-base');
                    if (dataAttr && normalizeBase(dataAttr)) {
                        console.log('[GatewayClient] Using script data attribute URL:', dataAttr);
                        return normalizeBase(dataAttr);
                    }
                }
            } catch (e) {
                console.error('[GatewayClient] Error reading meta/script:', e);
            }
        }

        if (normalizeBase(global.FEANE_GATEWAY_BASE_URL)) {
            console.log('[GatewayClient] Using global FEANE_GATEWAY_BASE_URL:', global.FEANE_GATEWAY_BASE_URL);
            return normalizeBase(global.FEANE_GATEWAY_BASE_URL);
        }

        try {
            var current = global.location ? new URL(global.location.href) : null;
            if (current) {
                var candidate = current.origin;
                console.log('[GatewayClient] Current origin:', current.origin, 'port:', current.port);
                if (current.port === '5003') {
                    candidate = candidate.replace(':5003', ':5000');
                    console.log('[GatewayClient] Detected frontend port 5003, switching to gateway port 5000');
                }
                console.log('[GatewayClient] Final candidate URL:', candidate);
                return normalizeBase(candidate);
            }
        } catch (e) {
            console.error('[GatewayClient] Error detecting from location:', e);
        }

        console.log('[GatewayClient] Using fallback URL: http://localhost:5000');
        return 'http://localhost:5000';
    }

    var DEFAULT_BASE_URL = detectBaseUrl();
    global.FEANE_GATEWAY_BASE_URL = DEFAULT_BASE_URL;
    console.log('[GatewayClient] Initialized with base URL:', DEFAULT_BASE_URL);

    function isObject(value) {
        return value !== null && typeof value === 'object' && !(value instanceof Date);
    }

    function isFormDataLike(value) {
        return typeof FormData !== 'undefined' && value instanceof FormData;
    }

    function isUrlSearchParams(value) {
        return typeof URLSearchParams !== 'undefined' && value instanceof URLSearchParams;
    }

    function mergeHeaders(target, source) {
        var result = {};
        Object.keys(target || {}).forEach(function (key) {
            result[key.toLowerCase()] = target[key];
        });

        Object.keys(source || {}).forEach(function (key) {
            result[key.toLowerCase()] = source[key];
        });

        var normalized = {};
        Object.keys(result).forEach(function (lowerKey) {
            var originalKey = lowerKey.split('-').map(function (part) {
                return part.charAt(0).toUpperCase() + part.slice(1);
            }).join('-');
            normalized[originalKey] = result[lowerKey];
        });

        return normalized;
    }

    function GatewayClient(baseUrl) {
        this.baseUrl = (baseUrl || DEFAULT_BASE_URL).replace(/\/$/, '');
    }

    GatewayClient.prototype.buildUrl = function (path) {
        if (!path) {
            return this.baseUrl;
        }

        if (/^https?:\/\//i.test(path)) {
            console.log('[GatewayClient] Using absolute URL:', path);
            return path;
        }

        if (path.charAt(0) !== '/') {
            path = '/' + path;
        }

        var finalUrl = this.baseUrl + path;
        console.log('[GatewayClient] Built URL:', finalUrl, '(base:', this.baseUrl, '+ path:', path + ')');
        return finalUrl;
    };

    GatewayClient.prototype.refreshToken = function () {
        var self = this;
        var refreshToken = localStorage.getItem('refreshToken');

        if (!refreshToken) {
            console.log('[GatewayClient] No refresh token available');
            return Promise.reject(new Error('No refresh token'));
        }

        console.log('[GatewayClient] Attempting to refresh token...');

        return fetch(self.buildUrl('/api/auth/refresh'), {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({refreshToken: refreshToken}),
            credentials: 'include'
        }).then(function (response) {
            return response.text().then(function (text) {
                var data = text ? JSON.parse(text) : null;

                if (response.ok && data && data.token) {
                    localStorage.setItem('jwtToken', data.token);
                    localStorage.setItem('jwt', data.token);
                    if (data.refreshToken) {
                        localStorage.setItem('refreshToken', data.refreshToken);
                    }
                    console.log('[GatewayClient] Token refreshed successfully');
                    return data.token;
                } else {
                    console.log('[GatewayClient] Token refresh failed');
                    throw new Error('Token refresh failed');
                }
            });
        });
    };

    GatewayClient.prototype.request = function (path, options) {
        var self = this;
        options = options || {};
        var url = this.buildUrl(path);
        var headers = mergeHeaders({'Accept': 'application/json'}, options.headers);
        var body = options.body;

        var lowerPath = typeof path === 'string' ? path.toLowerCase() : '';
        var skipAuthHeader = lowerPath.includes('/api/auth/login') ||
            lowerPath.includes('/api/auth/register') ||
            lowerPath.includes('/api/auth/refresh');

        if (!skipAuthHeader) {
            try {
                var token = localStorage.getItem('jwtToken') || localStorage.getItem('jwt');
                if (token && !headers['Authorization'] && !headers['authorization']) {
                    headers['Authorization'] = 'Bearer ' + token;
                    console.log('[GatewayClient] Added JWT token to request');
                }
            } catch (e) {
                console.warn('[GatewayClient] Failed to read JWT from localStorage:', e);
            }
        }

        if (isObject(body) && !isFormDataLike(body) && !isUrlSearchParams(body) && !(body instanceof Blob)) {
            headers['Content-Type'] = headers['Content-Type'] || 'application/json';
            body = JSON.stringify(body);
        }

        var fetchOptions = {
            method: options.method || 'GET',
            headers: headers,
            body: body,
            credentials: options.credentials || 'include'
        };

        return fetch(url, fetchOptions).then(function (response) {
            return response.text().then(function (text) {
                var data = null;
                if (text) {
                    try {
                        data = JSON.parse(text);
                    } catch (error) {
                        data = text;
                    }
                }

                if (!response.ok) {
                    // Handle 401 Unauthorized - try to refresh token
                    if (response.status === 401 && !skipAuthHeader && !options._isRetry) {
                        console.log('[GatewayClient] Got 401, attempting token refresh...');
                        return self.refreshToken().then(function (newToken) {
                            // Retry the request with new token
                            console.log('[GatewayClient] Retrying request with new token');
                            options._isRetry = true;
                            return self.request(path, options);
                        }).catch(function (refreshError) {
                            console.log('[GatewayClient] Token refresh failed, showing renewal dialog');
                            // Show token renewal dialog
                            return self.showTokenRenewalDialog().then(function (shouldRenew) {
                                if (shouldRenew) {
                                    return self.refreshToken().then(function () {
                                        options._isRetry = true;
                                        return self.request(path, options);
                                    });
                                } else {
                                    // User declined, redirect to login
                                    self.redirectToLogin();
                                    throw new Error('Unauthorized');
                                }
                            });
                        });
                    }

                    var errorMessage = (data && data.message) || response.statusText || 'Request failed';
                    var error = new Error(errorMessage);
                    error.status = response.status;
                    error.data = data;
                    throw error;
                }

                return data;
            });
        });
    };

    GatewayClient.prototype.showTokenRenewalDialog = function () {
        return new Promise(function (resolve) {
            var existingDialog = document.getElementById('token-renewal-dialog');
            if (existingDialog) {
                existingDialog.remove();
            }

            var dialog = document.createElement('div');
            dialog.id = 'token-renewal-dialog';
            dialog.innerHTML = `
                <div style="position: fixed; inset: 0; background: rgba(0,0,0,0.7); display: flex; align-items: center; justify-content: center; z-index: 99999;">
                    <div style="background: white; padding: 30px; border-radius: 15px; max-width: 400px; text-align: center; box-shadow: 0 10px 40px rgba(0,0,0,0.3);">
                        <div style="font-size: 48px; margin-bottom: 15px;">⏰</div>
                        <h3 style="margin: 0 0 10px 0; color: #333;">Session Expired</h3>
                        <p style="color: #666; margin-bottom: 25px;">Your session has expired. Would you like to renew it and continue?</p>
                        <div style="display: flex; gap: 10px;">
                            <button id="token-renew-btn" style="flex: 1; padding: 12px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; border: none; border-radius: 8px; font-weight: 600; cursor: pointer; font-size: 16px;">
                                Renew Session
                            </button>
                            <button id="token-login-btn" style="flex: 1; padding: 12px; background: #6c757d; color: white; border: none; border-radius: 8px; font-weight: 600; cursor: pointer; font-size: 16px;">
                                Login Again
                            </button>
                        </div>
                    </div>
                </div>
            `;

            document.body.appendChild(dialog);

            document.getElementById('token-renew-btn').onclick = function () {
                dialog.remove();
                resolve(true);
            };

            document.getElementById('token-login-btn').onclick = function () {
                dialog.remove();
                resolve(false);
            };
        });
    };

    GatewayClient.prototype.redirectToLogin = function () {
        var currentUrl = encodeURIComponent(window.location.pathname + window.location.search);
        window.location.href = '/account/authentication?returnUrl=' + currentUrl;
    };

    GatewayClient.prototype.get = function (path, options) {
        return this.request(path, mergeOptions(options, {method: 'GET'}));
    };

    GatewayClient.prototype.post = function (path, body, options) {
        return this.request(path, mergeOptions(options, {method: 'POST', body: body}));
    };

    GatewayClient.prototype.put = function (path, body, options) {
        return this.request(path, mergeOptions(options, {method: 'PUT', body: body}));
    };

    GatewayClient.prototype.delete = function (path, options) {
        return this.request(path, mergeOptions(options, {method: 'DELETE'}));
    };

    function mergeOptions(target, source) {
        var result = {};
        [target || {}, source || {}].forEach(function (optionSet) {
            Object.keys(optionSet).forEach(function (key) {
                if (key === 'headers') {
                    result.headers = mergeHeaders(result.headers, optionSet.headers);
                } else {
                    result[key] = optionSet[key];
                }
            });
        });
        return result;
    }

    global.GatewayClient = GatewayClient;
    global.feaneGateway = new GatewayClient(DEFAULT_BASE_URL);
})(window);
