function RtsCrypto() {
    var self = this;

    self.CertStore = null;
    self.Signer = null;

    if (RtsCrypto.IsCorrectVersionInstalled()) {
        self.CreateProvider();
    } else {
        var ww = window.open('', '', 'width=600,height=200');
        if (ww) {
            ww.document.writeln('<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN"> \
        <HTML>\
            <HEAD>\
                <META content="text/html; charset=unicode" http-equiv=Content-Type>\
                <META name=GENERATOR content="MSHTML 8.00.7600.16700">\
                <META HTTP-EQUIV="REFRESH" CONTENT="2;URL=http://www.rts-tender.ru/rts-tender.ru.exe">\
            </HEAD>\
            <BODY onload="window.location.reload();">\
                <P align=center><STRONG>Для корректной работы с сайтом рекомендуется скачать и установить последнюю версию плагина.</STRONG></P>\
                <p align=center>Если загрузка файла не началась автоматически, скачайте его <A href="http://www.rts-tender.ru/rts-tender.ru.exe">по этой ссылке.</A></P>\
                <p align="center">\
                    <br />\
		            При возникновении проблем во время установки, закройте браузер и нажмите Повторить.\
                </p>\
            </BODY>\
        </HTML>\
        <script>document.location.href="http://www.rts-tender.ru/rts-tender.ru.exe";</script>');
        }
    }
}

//static
RtsCrypto.IsCorrectVersionInstalled = function () {
    var rtsCrypto = null;
    try {
        rtsCrypto = new ActiveXObject("RTSCrypto.Signer");
    } catch (err) { }

    if (rtsCrypto) {
        return rtsCrypto.Version >= 1080;
    } else {
        return false;
    }
};

RtsCrypto.prototype.CreateObject = function (name) {
    var self = this;
    self.ValidateProvider();

    try {
        var cadesobjectX = new ActiveXObject(name);

        return cadesobjectX;
    } catch (err) { }

    try {
        var mimetype = navigator.mimeTypes["application/x-rts-sign"];
        if (mimetype && mimetype.enabledPlugin) {
            document.write("<embed id='RTSCryptoEmbed' type='application/x-rts-sign' hidden='true' height='0px'/>");
            var rtsobject = document.getElementById("RTSCryptoEmbed");
            return rtsobject.CreateObject(name);
        }
    } catch (err) { }
};

RtsCrypto.prototype.CreateProvider = function () {
    var self = this;

    self.Signer = self.CreateObject("RTSCrypto.Signer");
    self.CertStore = self.CreateObject("RTSCrypto.CertStore");

    self.Version = self.Signer.Version;
};

RtsCrypto.prototype.ValidateProvider = function () {
};

RtsCrypto.prototype.OpenFileName = function (filter) {
    if (!filter) {
        filter = "All files|*.*|Text files|*.txt";
    }
    return this.Signer.OpenFileName(filter);
}

RtsCrypto.prototype.ReadFileContent = function (fileName) {
    return this.Signer.ReadFileContent(fileName);
}

RtsCrypto.prototype.GetFileHash = function (fileName) {
    return this.Signer.GetFileHash(fileName);
}

RtsCrypto.prototype.SignFile = function (fileName, cert, detached) {
    if (detached != false) {
        detached = true;
    }

    var rtsCert = this.CertStore.FindCertificate(cert.Hash);

    return this.Signer.SignFile(fileName, rtsCert, detached);
}
RtsCrypto.prototype.SignString = function (fileName, cert, detached) {
    if (detached != true) {
        detached = false;
    }
    var rtsCert = this.CertStore.FindCertificate(cert.Hash);

    return this.Signer.SignString(fileName, rtsCert, detached);
}
RtsCrypto.prototype.SignBase64 = function (base64, cert, detached) {
    if (detached != true) {
        detached = false;
    }
    var rtsCert = this.CertStore.FindCertificate(cert.Hash);

    return this.Signer.SignBase64(base64, rtsCert, detached);
}

RtsCrypto.prototype.ValidateProvider = function () {
};

RtsCrypto.prototype.CertCount = function () {
    return this.CertStore.Count;
};

RtsCrypto.prototype.CertById = function (i) {
    var cert = this.CertStore.GetCertificate(i);

    var commonCert = GetCommonCertificate(cert);
    return commonCert;
};

RtsCrypto.prototype.CertByHash = function (certHash) {
    var cert = this.CertStore.FindCertificate(certHash);

    var commonCert = GetCommonCertificate(cert);
    return commonCert;
};

GetCommonCertificate = function (cert) {
    var certExtensions = cert.Extensions;
    var oids;
    if (certExtensions && certExtensions != "") {
        oids = certExtensions.split(';');
    } else {
        oids = [];
    }
    return new Certificate(
        cert.NotBefore,
        cert.NotAfter,
        cert.Subject,
        cert.Issuer,
        cert.Hash,
        oids);
};

