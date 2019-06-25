function Certificate(validAfter, validBefore, subject, issuer, hash, oids) {
    var self = this;
    self.ValidAfter = validAfter;
    self.ValidBefore = validBefore;
    self.Subject = subject;
    self.Issuer = issuer;
    self.Hash = hash;
    self.Oids = oids;
    self.DisplayValidDate ="c "+this.formatDate(validAfter) + ' по ' + this.formatDate(validBefore);
    if (validBefore < new Date())
        self.DisplayValidDate += " (не активен)";
}


Certificate.prototype.formatDate = function (strDate) {
    var date = new Date(strDate);
    var month = date.getMonth() + 1;
    var day = date.getDate();

    var output = (('' + day).length < 2 ? '0' : '') + day + '-' +
        (('' + month).length < 2 ? '0' : '') + month + '-' +
        date.getFullYear();
    return output;
};

function CryptoProBrowserPluginProvider() {
    var self = this;
    self.RTSCrypto = null;

    self.Store = null;
    self.Signer = null;

    if (CryptoProBrowserPluginProvider.IsPluginInstalled()) {
        self.CreateProvider();
    } else {
        var ww = window.open('', '', 'width=600,height=200');
        if (ww) {
            ww.document.writeln('<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN"> \
        <HTML>\
            <HEAD>\
                <META content="text/html; charset=unicode" http-equiv=Content-Type>\
                <META name=GENERATOR content="MSHTML 8.00.7600.16700">\
                <META HTTP-EQUIV="REFRESH" CONTENT="2;URL=http://www.cryptopro.ru/products/cades/plugin/get">\
            </HEAD>\
            <BODY onload="window.location.reload();">\
                <P align=center><STRONG>Для корректной работы с сайтом рекомендуется скачать и установить последнюю версию плагина.</STRONG></P>\
                <p align=center>Если загрузка файла не началась автоматически, скачайте его <A href="http://www.cryptopro.ru/products/cades/plugin/get">по этой ссылке.</A></P>\
                <p align="center">\
                    <br />\
		            При возникновении проблем во время установки, закройте браузер и нажмите Повторить.\
                </p>\
            </BODY>\
        </HTML>\
        <script>document.location.href="http://www.cryptopro.ru/products/cades/plugin/get";</script>');
        }
    }
}

//constants
$.extend(CryptoProBrowserPluginProvider, {
    CADESCOM_CADES_X_LONG_TYPE_1 : 0x5d,
    CAPICOM_CURRENT_USER_STORE : 2,
    CAPICOM_MY_STORE : "My",
    CAPICOM_STORE_OPEN_MAXIMUM_ALLOWED : 2,
    CAPICOM_CERTIFICATE_FIND_SUBJECT_NAME : 1,
    CAPICOM_CERTIFICATE_FIND_SHA1_HASH : 0,
    CADES_BES : 1, //не улучшенная эцп
    CADESCOM_CADES_DEFAULT : 0,
    CAPICOM_CERTIFICATE_INCLUDE_CHAIN_EXCEPT_ROOT : 0,
    CAPICOM_CERTIFICATE_INCLUDE_WHOLE_CHAIN : 1,
    CAPICOM_CERTIFICATE_INCLUDE_END_ENTITY_ONLY : 2,
    CADESCOM_STRING_TO_UCS2LE : 0x00, //Данные будут перекодированы в UCS-2 little endian.
    CADESCOM_BASE64_TO_BINARY : 0x01, //Данные будут перекодированы из Base64 в бинарный массив.

    CAPICOM_STORE_OPEN_READ_ONLY : 0,
    MICROSOFT_INTERNET_EXPLORER : 'Microsoft Internet Explorer',
    CAPICOM_ENCODE_BASE64 : 0,
    CAPICOM_VERIFY_SIGNATURE_ONLY : 0,

    CAPICOM_INFO_SUBJECT_SIMPLE_NAME : 0,
    CAPICOM_INFO_ISSUER_SIMPLE_NAME : 1,
    CAPICOM_INFO_SUBJECT_EMAIL_NAME : 2,
    CAPICOM_INFO_ISSUER_EMAIL_NAME : 3
});

//static
CryptoProBrowserPluginProvider.IsPluginInstalled = function () {
    var testedVersion = {
        Major: 1,
        Minor: 5,
        Build: 1500
    };

    var cadescomAbout = null;
    try {
        cadescomAbout = new ActiveXObject("CAdESCOM.About");
    } catch (err) { }

    if (cadescomAbout == null) {
        try {
            var mimetype = navigator.mimeTypes["application/x-cades"];
            if (mimetype && mimetype.enabledPlugin) {
                cadescomAbout = document.getElementById("cadesplugin").CreateObject("CAdESCOM.About");
            }
        } catch (err) { }
    }

    if (cadescomAbout) {
        return (cadescomAbout.MajorVersion > testedVersion.Major)
            || (cadescomAbout.MajorVersion === testedVersion.Major && cadescomAbout.MinorVersion > testedVersion.Minor)
            || (cadescomAbout.MajorVersion === testedVersion.Major && cadescomAbout.MinorVersion === testedVersion.Minor && cadescomAbout.BuildVersion >= testedVersion.Build);
    } else {
        return false;
    }
};

CryptoProBrowserPluginProvider.prototype.CreateObject = function (name) {
    var self = this;
    self.ValidateProvider();

    try {
        var cadesobjectX = new ActiveXObject(name);

        return cadesobjectX;
    } catch (err) { }

    try {
        var mimetype = navigator.mimeTypes["application/x-cades"];
        if (mimetype && mimetype.enabledPlugin) {
            var cadesobject = document.getElementById("cadesplugin");
            return cadesobject.CreateObject(name);
        }
    } catch (err) { }
};

CryptoProBrowserPluginProvider.prototype.CreateProvider = function () {
    var self = this;
    // Создание объектов КриптоПро ЭЦП Browser plug-in
    self.Store = self.CreateObject("CAPICOM.Store");
    self.Signer = self.CreateObject("CAdESCOM.CPSigner");
};

CryptoProBrowserPluginProvider.prototype.ValidateProvider = function () {
    if (!CryptoProBrowserPluginProvider.IsPluginInstalled()) {
        throw new Error("Не создан объект КриптоПро ЭЦП Browser plug-in. Не установлен КриптоПро ЭЦП Browser plug-in. Установите и проверьте плагин http://www.cryptopro.ru/cadesplugin/Default.aspx");
    }
};

CryptoProBrowserPluginProvider.prototype.SignString = function (toSign, currentCertificat, isDetached) {
    var self = this;

    self.Store.Open(CryptoProBrowserPluginProvider.CAPICOM_CURRENT_USER_STORE, CryptoProBrowserPluginProvider.CAPICOM_MY_STORE, CryptoProBrowserPluginProvider.CAPICOM_STORE_OPEN_MAXIMUM_ALLOWED);
    var oCertificates = self.Store.Certificates.Find(CryptoProBrowserPluginProvider.CAPICOM_CERTIFICATE_FIND_SHA1_HASH, currentCertificat.Hash);
    if (oCertificates.Count == 0) {
        throw new Error("Certificate not found");
    }

    var oCertificate = oCertificates.Item(1);
    self.Signer.Certificate = oCertificate;
    //oSigner.TSAAddress = "http://cryptopro.ru/tsp/";

    var oSignedData = self.CreateObject("CAdESCOM.CadesSignedData");
    oSignedData.Content = toSign;
    self.Signer.Options = CryptoProBrowserPluginProvider.CAPICOM_CERTIFICATE_INCLUDE_END_ENTITY_ONLY;
    try {
        var sSignedMessage = oSignedData.SignCades(self.Signer, CryptoProBrowserPluginProvider.CADES_BES, isDetached);
    } catch (err) {
        throw new Error("Failed to create signature. Error: " + self.GetErrorMessage(err));
    }

    self.Store.Close();

    return sSignedMessage;
};

CryptoProBrowserPluginProvider.prototype.SignBase64 = function (toSign, currentCertificat, isDetached) {
    var self = this;

    self.Store.Open(CryptoProBrowserPluginProvider.CAPICOM_CURRENT_USER_STORE, CryptoProBrowserPluginProvider.CAPICOM_MY_STORE, CryptoProBrowserPluginProvider.CAPICOM_STORE_OPEN_MAXIMUM_ALLOWED);
    var oCertificates = self.Store.Certificates.Find(CryptoProBrowserPluginProvider.CAPICOM_CERTIFICATE_FIND_SHA1_HASH, currentCertificat.Hash);
    if (oCertificates.Count == 0) {
        throw new Error("Certificate not found");
    }

    var oCertificate = oCertificates.Item(1);
    self.Signer.Certificate = oCertificate;
    //oSigner.TSAAddress = "http://cryptopro.ru/tsp/";

    var oSignedData = self.CreateObject("CAdESCOM.CadesSignedData");
    oSignedData.ContentEncoding = CryptoProBrowserPluginProvider.CADESCOM_BASE64_TO_BINARY;
    oSignedData.Content = toSign;
    self.Signer.Options = CryptoProBrowserPluginProvider.CAPICOM_CERTIFICATE_INCLUDE_END_ENTITY_ONLY;
    try {
        var sSignedMessage = oSignedData.SignCades(self.Signer, CryptoProBrowserPluginProvider.CADES_BES, isDetached);
    } catch (err) {
        throw new Error("Failed to create signature. Error: " + self.GetErrorMessage(err));
    }

    self.Store.Close();

    return sSignedMessage;
};

CryptoProBrowserPluginProvider.prototype.CertCount = function () {
    var self = this,
        result;
    try {
        self.Store.Open(CryptoProBrowserPluginProvider.CAPICOM_CURRENT_USER_STORE, CryptoProBrowserPluginProvider.CAPICOM_MY_STORE, CryptoProBrowserPluginProvider.CAPICOM_STORE_OPEN_MAXIMUM_ALLOWED);
        result = self.Store.Certificates.Count;
    } catch (e) {
        result = 0;
    }
    return result;
};

CryptoProBrowserPluginProvider.prototype.CertByHash = function (certHash) {
    var self = this;

    self.Store.Open(CryptoProBrowserPluginProvider.CAPICOM_CURRENT_USER_STORE, CryptoProBrowserPluginProvider.CAPICOM_MY_STORE, CryptoProBrowserPluginProvider.CAPICOM_STORE_OPEN_MAXIMUM_ALLOWED);
    var oCertificates = self.Store.Certificates.Find(CryptoProBrowserPluginProvider.CAPICOM_CERTIFICATE_FIND_SHA1_HASH, certHash);
    if (oCertificates.Count == 0) {
        throw new Error("Certificate not found");
    }

    var cert = oCertificates.Item(1);
    var extendedUsage = cert.ExtendedKeyUsage();
    var oids = [];
    if (extendedUsage && extendedUsage.EKUs && extendedUsage.EKUs.Count >0) {
        for (var i = 1; i <= extendedUsage.EKUs.Count; i++) {
            var ekuItem = extendedUsage.EKUs.Item(i);
            if (ekuItem && ekuItem.OID) {
                oids.push(ekuItem.OID);
            }
        }
    }
    var commonCert = new Certificate(
        cert.ValidFromDate,
        cert.ValidToDate,
        cert.GetInfo(CryptoProBrowserPluginProvider.CAPICOM_INFO_SUBJECT_SIMPLE_NAME),
        cert.GetInfo(CryptoProBrowserPluginProvider.CAPICOM_INFO_ISSUER_SIMPLE_NAME),
        cert.Thumbprint,
        oids);
    return commonCert;
};

CryptoProBrowserPluginProvider.prototype.CertById = function (i) {
    var self = this;
    self.Store.Open(
        CryptoProBrowserPluginProvider.CAPICOM_CURRENT_USER_STORE,
        CryptoProBrowserPluginProvider.CAPICOM_MY_STORE,
        CryptoProBrowserPluginProvider.CAPICOM_STORE_OPEN_MAXIMUM_ALLOWED);
    var cert = self.Store.Certificates.Item(i + 1); //нумерация с 1 

    var extendedUsage = cert.ExtendedKeyUsage();
    var oids = [];
    if (extendedUsage && extendedUsage.EKUs && extendedUsage.EKUs.Count > 0) {
        for (var i = 1; i <= extendedUsage.EKUs.Count; i++) {
            var ekuItem = extendedUsage.EKUs.Item(i);
            if (ekuItem && ekuItem.OID) {
                oids.push(ekuItem.OID);
            }
        }
    }

    var commonCert = new Certificate(
        cert.ValidFromDate,
        cert.ValidToDate,
        cert.GetInfo(CryptoProBrowserPluginProvider.CAPICOM_INFO_SUBJECT_SIMPLE_NAME),
        cert.GetInfo(CryptoProBrowserPluginProvider.CAPICOM_INFO_ISSUER_SIMPLE_NAME),
        cert.Thumbprint,
        oids);
    return commonCert;
};

CryptoProBrowserPluginProvider.prototype.VerifyBase64 = function(sSignedMessage, dataToVerify) {
    var oSignedData = this.CreateObject("CAdESCOM.CadesSignedData");
    try {
        oSignedData.ContentEncoding = CryptoProBrowserPluginProvider.CADESCOM_BASE64_TO_BINARY;
        oSignedData.Content = dataToVerify;
        oSignedData.VerifyCades(sSignedMessage, CryptoProBrowserPluginProvider.CADES_BES, true);
    } catch (err) {
        
        console.log("Failed to verify signature. Error: " + this.GetErrorMessage(err));
        return false;
    }

    return true;
};


CryptoProBrowserPluginProvider.prototype.GetErrorMessage = function (e) {
    var err = e.message;
    if (!err) {
        err = e;
    } else if (e.number) {
        err += " (0x" + this.decimalToHexString(e.number) + ")";
    }
    return err;
};

CryptoProBrowserPluginProvider.prototype.decimalToHexString = function (number) {
    if (number < 0) {
        number = 0xFFFFFFFF + number + 1;
    }

    return number.toString(16).toUpperCase();
};
