mergeInto(LibraryManager.library, {
    GetTokenFromLocalStorage: function() {
        try {
            var token = localStorage.getItem('token') || localStorage.getItem('authToken') || localStorage.getItem('accessToken');
            if (token) {
                var bufferSize = lengthBytesUTF8(token) + 1;
                var buffer = _malloc(bufferSize);
                stringToUTF8(token, buffer, bufferSize);
                return buffer;
            }
            return null;
        } catch (e) {
            console.error('Error getting token from localStorage:', e);
            return null;
        }
    },

    SetTokenToLocalStorage: function(token) {
        try {
            var tokenStr = UTF8ToString(token);
            localStorage.setItem('token', tokenStr);
            console.log('Token saved to localStorage');
        } catch (e) {
            console.error('Error saving token to localStorage:', e);
        }
    },

    RemoveTokenFromLocalStorage: function() {
        try {
            localStorage.removeItem('token');
            localStorage.removeItem('authToken');
            localStorage.removeItem('accessToken');
            console.log('Token removed from localStorage');
        } catch (e) {
            console.error('Error removing token from localStorage:', e);
        }
    }
});