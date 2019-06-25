// Ансинхронный провайдер для криптопро
var cryptoProBrowserPluginProvider = function() {

    var constants = {
        CADESCOM_CADES_X_LONG_TYPE_1: 0x5d,
        CAPICOM_CURRENT_USER_STORE: 2,
        CAPICOM_MY_STORE: "My",
        CAPICOM_STORE_OPEN_MAXIMUM_ALLOWED: 2,
        CAPICOM_CERTIFICATE_FIND_SUBJECT_NAME: 1,
        CAPICOM_CERTIFICATE_FIND_SHA1_HASH: 0,
        CADES_BES: 1, //не улучшенная эцп
        CADESCOM_CADES_DEFAULT: 0,
        CAPICOM_CERTIFICATE_INCLUDE_CHAIN_EXCEPT_ROOT: 0,
        CAPICOM_CERTIFICATE_INCLUDE_WHOLE_CHAIN: 1,
        CAPICOM_CERTIFICATE_INCLUDE_END_ENTITY_ONLY: 2,
        CADESCOM_STRING_TO_UCS2LE: 0x00, //Данные будут перекодированы в UCS-2 little endian.
        CADESCOM_BASE64_TO_BINARY: 0x01, //Данные будут перекодированы из Base64 в бинарный массив.

        CAPICOM_STORE_OPEN_READ_ONLY: 0,
        MICROSOFT_INTERNET_EXPLORER: 'Microsoft Internet Explorer',
        CAPICOM_ENCODE_BASE64: 0,
        CAPICOM_VERIFY_SIGNATURE_ONLY: 0,

        CAPICOM_INFO_SUBJECT_SIMPLE_NAME: 0,
        CAPICOM_INFO_ISSUER_SIMPLE_NAME: 1,
        CAPICOM_INFO_SUBJECT_EMAIL_NAME: 2,
        CAPICOM_INFO_ISSUER_EMAIL_NAME: 3,

        CADESCOM_HASH_ALGORITHM_CP_GOST_3411: 100,
        CADESCOM_HASH_ALGORITHM_SHA1: 0
    };

    var getCertificate = function (cert) {
        var subject = cert.SubjectName,
            firstNameMatch,
            surnameMatch,
            middleNameMatch;

        if (subject.match(/G\s*=/)) {
            firstNameMatch = subject.match(
                /G\s*=[\s"]*([а-яА-ЯёЁ\w-.]+)/);
            surnameMatch = subject.match(
                /SN\s*=[\s"]*([а-яА-ЯёЁ\w-.]+)/);
            middleNameMatch = subject.match(
                /G\s*=[\s"]*[а-яА-ЯёЁ\w-.]+\s+(([а-яА-ЯёЁ\w-.\s]+))/);
        } else {
            firstNameMatch = subject.match(
                /CN\s*=[\s"]*[а-яА-ЯёЁ\w-.]+\s+([а-яА-ЯёЁ\w-.]+)/);
            surnameMatch = subject.match(
                /CN\s*=[\s"]*([а-яА-ЯёЁ\w-.]+)/);
            middleNameMatch = subject.match(
                /CN\s*=[\s"]*[а-яА-ЯёЁ\w-.]+\s+[а-яА-ЯёЁ\w-.]+\s+(([а-яА-ЯёЁ\w-.\s]+))/
            );

        }

        var names = [surnameMatch, firstNameMatch, middleNameMatch];
        for (var j = 0; j < names.length; j++) {
            names[j] = (names[j] && names[j][1]) ? names[j][1] : null;
        }

        var organization = '';
        if (subject.match(/O\s*=/)) {
            organization = subject.match(/O\s*=[\s"]*[^,]+/)[0];
            var startPosEq = organization.search('=');
            organization = organization.substring(startPosEq + 1,
                organization.length - startPosEq + 2);
        }

        if (organization === '') {
            organization = cert.GetInfo(constants.CAPICOM_INFO_SUBJECT_SIMPLE_NAME);
        }
        var role = "Не заказчик по 44-ФЗ"; //по умолчанию
        var isNotCustomer = true;

        var extendedUsage = cert.ExtendedKeyUsage();

        var coids = [];
        if (extendedUsage && extendedUsage.EKUs && extendedUsage.EKUs.Count > 0) {
            for (var i = 1; i <= extendedUsage.EKUs.Count; i++) {
                var ekuItem = extendedUsage.EKUs.Item(i);
                if (ekuItem && ekuItem.OID) {
                    coids.push(ekuItem.OID);
                }
            }
        }

        if (coids.length !== 0) {
            if (cryptoHelper.isStartsWithOid(coids, cryptoHelper.oids.id_eku_GF05_authorized.oid))
                role = "Уполномоченный орган";
            else if (cryptoHelper.isStartsWithOid(coids, cryptoHelper.oids.id_eku_GF05_authorizedInstitute.oid))
                role = "Уполномоченное учреждение";
            else if (cryptoHelper.isStartsWithOid(coids, cryptoHelper.oids.id_eku_GF05_customer.oid)) {
                role = "Заказчик";
                isNotCustomer = false;
            } else if (cryptoHelper.isStartsWithOid(coids, cryptoHelper.oids.id_eku_GF05_specializedOrg.oid))
                role = "Специализированная организация";
            else if (cryptoHelper.isStartsWithOid(coids, cryptoHelper.oids.id_eku_GF05_supervising.oid))
                role = "Контролирующий орган";
            else if (cryptoHelper.isStartsWithOid(coids, cryptoHelper.oids.id_eku_GF05.oid))
                role = "Прочий";
        }

        var commonCert = new Certificate(
             cert.ValidFromDate,
             cert.ValidToDate,
            names.join(' '),
             cert.GetInfo(constants.CAPICOM_INFO_ISSUER_SIMPLE_NAME),
             cert.Thumbprint,
            organization, role, isNotCustomer);
        return commonCert;
    };

    var createObject = function (name) {
        return cadesplugin.CreateObject(name);
    };


    var usedStoreCallDepth = 0;
    var usedStoreInstance;

    var usedStore = function(func) {
        usedStoreCallDepth += 1;

        if (usedStoreCallDepth === 1) {
            usedStoreInstance = createObject("CAPICOM.Store");
            usedStoreInstance.Open(constants.CAPICOM_CURRENT_USER_STORE, constants.CAPICOM_MY_STORE, constants.CAPICOM_STORE_OPEN_MAXIMUM_ALLOWED);
        }

        var result = func(usedStoreInstance);

        if (usedStoreCallDepth === 1) {
            usedStoreInstance.Close();
        }

        usedStoreCallDepth -= 1;
        return Promise.resolve(result);
    };

    var certCount = function() {
        return usedStore(function (store) {
            return Promise.resolve(store.Certificates.Count);
        });
    };

    var certById = function(id) {
        return usedStore(function(store) {
            var cert = store.Certificates.Item(id + 1);
            return Promise.resolve(getCertificate(cert));
        });
    };

    var getCertificateFromStoreByHash = function (store, certificateHash) {
        var certificates = store.Certificates.Find(constants.CAPICOM_CERTIFICATE_FIND_SHA1_HASH, certificateHash);
        if (certificates.Count === 0) {
            throw new Error("Certificate not found");
        }

        return certificates.Item(1);
    };

    var certByHash = function (certHash) {
        return usedStore(function(store) {
            var certificate = getCertificateFromStoreByHash(store, certHash);

            return Promise.resolve(getCertificate(certificate));
        });
    };

    var sign = function(content, contentEncoding, certificatePromise, isDetached) {
        return certificatePromise.then(function(currentCertificate) {
            return usedStore(function(store) {
                var certificate = getCertificateFromStoreByHash(store, currentCertificate.Hash);

                var signer = createObject("CAdESCOM.CPSigner");
                signer.Certificate = certificate;

                var oSignedData = createObject("CAdESCOM.CadesSignedData");

                oSignedData.ContentEncoding = contentEncoding;
                oSignedData.Content = content;
                signer.Options = constants.CAPICOM_CERTIFICATE_INCLUDE_END_ENTITY_ONLY;

                var sSignedMessage = oSignedData.SignCades(signer, constants.CADES_BES, isDetached);
                return sSignedMessage;
            });
        });
    };

    var signBase64 = function (toSign, currentCertificate, isDetached) {
        return sign(toSign, constants.CADESCOM_BASE64_TO_BINARY, currentCertificate, isDetached);
    };

    var signString = function (toSign, currentCertificate, isDetached) {
        return sign(toSign, constants.CADESCOM_STRING_TO_UCS2LE, currentCertificate, isDetached);
    };

    var isCertHasGostAlgoritm = function(certificateHash) {
        return usedStore(function(store) {
            var certificate = getCertificateFromStoreByHash(store, certificateHash);
            var publicKey = certificate.PublicKey();

            return Promise.resolve(publicKey != null &&
                publicKey.Algorithm != null &&
                (publicKey.Algorithm.FriendlyName.toUpperCase().indexOf("ГОСТ") >= 0 ||
                    publicKey.Algorithm.FriendlyName.toUpperCase().indexOf("GOST") >= 0));
        });
    };

    var getCertificates = function () {
        return usedStore(function(store) {
            var certificationCount = store.Certificates.Count;
            var certificates = [];
            for (var index = 1; index <= certificationCount; index++) {
                certificates.push(getCertificate(store.Certificates.Item(index)));
            }

            return certificates;
        });
    };

    var initializeHashedData = function (sHashValue) {
        // Создаем объект CAdESCOM.HashedData
        var cryptoProHashedData = createObject("CAdESCOM.HashedData");

        // Инициализируем объект заранее вычисленным хэш-значением
        // Алгоритм хэширования нужно указать до того, как будет передано хэш-значение
        cryptoProHashedData.Algorithm = constants.CADESCOM_HASH_ALGORITHM_CP_GOST_3411;
        cryptoProHashedData.SetHashValue(sHashValue);

        return cryptoProHashedData;
    };

    var signHash = function(hashToSign, currentCertificate) {
        return currentCertificate.then(function (certificate) {
            return usedStore(function(store) {
                var cryptopro$$Certificate = getCertificateFromStoreByHash(store, certificate.Hash);
                var cryptopro$$HashedData = initializeHashedData(hashToSign);

                var cryptopro$$Signer = createObject("CAdESCOM.CPSigner");
                cryptopro$$Signer.Certificate = cryptopro$$Certificate;
                cryptopro$$Signer.Options = constants.CAPICOM_CERTIFICATE_INCLUDE_END_ENTITY_ONLY;
                var cryptopro$$SignedData = createObject("CAdESCOM.CadesSignedData");
                var sSignedMessage = cryptopro$$SignedData.SignHash(cryptopro$$HashedData, cryptopro$$Signer, constants.CADES_BES);

                return sSignedMessage;
            }).then(null, function(error) { console.log(error); }) ;
        });
    };

    var provider = {
        getCertificates: getCertificates,
        сertCount: certCount,
        certById: certById,
        certByHash: certByHash,
        signBase64: signBase64,
        signHash: signHash,
        signString: signString,
        isCertHasGostAlgoritm: isCertHasGostAlgoritm
    };

    return new Promise(function(resolve, reject) {
        resolve(provider);
    });
};

cryptoProviderFactory.syncResolve();