function Certificate(validAfter, validBefore, subject, issuer, hash, organization, role, isNotCustomer) {
    var self = this;
    self.ValidAfter = validAfter;
    self.ValidBefore = validBefore;
    self.Subject = subject;
    self.Issuer = issuer;
    self.Hash = hash;
    self.Organization = organization;
    self.Role = role;
    self.IsNotCustomer = isNotCustomer;

    self.IsValid = function () {
        var curDate = new Date();

        var validAfterDate = typeof self.ValidAfter === 'string' ? new Date(self.ValidAfter) : self.ValidAfter;
        var validBeforeDate = typeof self.ValidBefore === 'string' ? new Date(self.ValidBefore) : self.ValidBefore;

        return curDate > validAfterDate && curDate < validBeforeDate;
    }

    var padZero = function(str, cnt) {
        rc = str.toString();
        while (rc.length < cnt)
            rc = "0" + rc;
        return rc;
    };

    var dateShortToString = function (dt) {
        var rc = new Date(dt);
        return padZero(rc.getDate(), 2) + "." + padZero(rc.getMonth() + 1, 2) + "." + rc.getFullYear();
    };

    self.CertificateValidationPeriod = function () {
        var dateStr = dateShortToString(self.ValidAfter) + '&nbsp;-&nbsp;' + dateShortToString(self.ValidBefore);

        if (!self.IsValid())
            dateStr += ' (Не активный)';

        return dateStr;
    };
}

Certificate.prototype.formatDate = function (strDate) {
    var date = new Date(strDate);
    var month = date.getMonth() + 1;
    var day = date.getDate();
    var hour = date.getHours();
    var minute = date.getMinutes();
    var second = date.getSeconds();

    var output = date.getFullYear() + '-' +
        (('' + month).length < 2 ? '0' : '') + month + '-' +
        (('' + day).length < 2 ? '0' : '') + day + ' ';
    //(('' + hour).length < 2 ? '0' : '') + hour + ':' +
    //(('' + minute).length < 2 ? '0' : '') + minute + ':' +
    //(('' + second).length < 2 ? '0' : '') + second;
    return output;
};


;
(function (cadesplugin) {

    var asyncCodeIncluded = 0;
    var asyncPromise;

    var syncCodeIncluded = 0;
    var syncPromise;

    var loadcryptoProBrowserPluginProvider = function(scriptUrl) {
        var fileref = document.createElement("script");
        fileref.setAttribute("type", "text/javascript");
        fileref.setAttribute("src", scriptUrl);
        document.getElementsByTagName("head")[0].appendChild(fileref);
    };

    function includeAsyncCode() {
        if (asyncCodeIncluded) {
            return asyncPromise;
        }

        asyncPromise = new Promise(function (resolve, reject) {
            window.cryptoProviderFactory.asyncResolve = resolve;
        });

        loadcryptoProBrowserPluginProvider(window.cryptoProBrowserPluginProviderAsyncUrl);

        asyncCodeIncluded = 1;

        return asyncPromise;
    }

    function includeSyncCode() {
        if (syncCodeIncluded) {
            return syncPromise;
        }

        syncPromise = new Promise(function (resolve, reject) {
            window.cryptoProviderFactory.syncResolve = resolve;
        });

        loadcryptoProBrowserPluginProvider(window.cryptoProBrowserPluginProviderSyncUrl);

        syncCodeIncluded = 1;

        return syncPromise;
    }

    window.cryptoProviderFactory = function() {
        var createProvider = function() {
            
            // TODO: временно убрана поддержка RtsCrypto.
            //if (typeof RtsCrypto !== "undefined" && RtsCrypto.IsCorrectVersionInstalled()) {
            //    return results;
            //}
            return cadesplugin.then(function() {
                if (!!cadesplugin.CreateObjectAsync) {

                    return includeAsyncCode().then(function() {
                        return cryptoProBrowserPluginProvider();
                    });
                }

                if (!!cadesplugin.CreateObject) {
                    return includeSyncCode().then(function() {
                        return cryptoProBrowserPluginProvider();
                    });
                }

                throw "Синхронная версия криптопро провайдера не поддерживается";
            }).then(null, function (error) {
                if (error === 'Плагин недоступен' || error === 'Истекло время ожидания загрузки плагина') {
                    $('#pluginUndefinedModel').arcticmodal();
                    var ww = window.open('', '', 'width=600,height=200');
                    ww.document.writeln('<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN"> \
        <HTML>\
            <HEAD>\
                <META content="text/html; charset=unicode" http-equiv=Content-Type>\
                <META name=GENERATOR content="MSHTML 8.00.7600.16700">\
                <META HTTP-EQUIV="REFRESH" CONTENT="2;URL=http://www.cryptopro.ru/products/cades/plugin/get_2_0">\
            </HEAD>\
            <BODY onload="window.location.reload();">\
                <P align=center><STRONG>Для корректной работы с сайтом рекомендуется скачать и установить последнюю версию плагина.</STRONG></P>\
                <p align=center>Если загрузка файла не началась автоматически, скачайте его <A href="http://www.cryptopro.ru/products/cades/plugin/get_2_0">по этой ссылке.</A></P>\
                <p align="center">\
                    <br />\
                    При возникновении проблем во время установки, закройте браузер и нажмите Повторить.\
                </p>\
            </BODY>\
        </HTML>\
        <script>document.location.href="http://www.cryptopro.ru/products/cades/plugin/get_2_0";</script>');
                } else {
                    console.log(error);
                }
            });
        };

        return {
            create: createProvider
        };
    }();
})(cadesplugin);
