(function () {
    'use strict';

    // Wait for Swagger UI's global `ui` object to be initialised before hooking.
    var timer = setInterval(function () {
        if (window.ui) {
            clearInterval(timer);
            installHook();
        }
    }, 200);

    function installHook() {
        var realFetch = window.fetch.bind(window);
        window.fetch = function (resource, init) {
            var p = realFetch(resource, init);
            var url = (typeof resource === 'string') ? resource : (resource && resource.url);
            if (url && url.toLowerCase().includes('/auth/token')) {
                p.then(function (res) {
                    if (res.ok) {
                        res.clone().json().then(function (body) {
                            if (body && body.token) {
                                window.ui.preauthorizeApiKey('Bearer', body.token);
                            }
                        }).catch(function () {});
                    }
                }).catch(function () {});
            }
            return p;
        };
    }
}());
