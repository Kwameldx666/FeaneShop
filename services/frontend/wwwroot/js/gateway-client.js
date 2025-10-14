(function (global) {
    'use strict';

    var DEFAULT_BASE_URL = (global.FEANE_GATEWAY_BASE_URL || 'http://localhost:5000').replace(/\/$/, '');

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
            return path;
        }

        if (path.charAt(0) !== '/') {
            path = '/' + path;
        }

        return this.baseUrl + path;
    };

    GatewayClient.prototype.request = function (path, options) {
        options = options || {};
        var url = this.buildUrl(path);
        var headers = mergeHeaders({ 'Accept': 'application/json' }, options.headers);
        var body = options.body;

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

    GatewayClient.prototype.get = function (path, options) {
        return this.request(path, mergeOptions(options, { method: 'GET' }));
    };

    GatewayClient.prototype.post = function (path, body, options) {
        return this.request(path, mergeOptions(options, { method: 'POST', body: body }));
    };

    GatewayClient.prototype.put = function (path, body, options) {
        return this.request(path, mergeOptions(options, { method: 'PUT', body: body }));
    };

    GatewayClient.prototype.delete = function (path, options) {
        return this.request(path, mergeOptions(options, { method: 'DELETE' }));
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
