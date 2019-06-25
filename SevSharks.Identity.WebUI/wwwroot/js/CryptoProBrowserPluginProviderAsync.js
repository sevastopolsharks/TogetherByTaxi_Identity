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

  var createObject = function(name) {
    var cadesObject = cadesplugin.then(function() {
      return cadesplugin.CreateObjectAsync(name);
    });
    return cadesObject;
  };

    var usedStore = function(action) {
        return cadesplugin.async_spawn(function*() {
            var store = yield createObject("CAPICOM.Store");
            yield store.Open(constants.CAPICOM_CURRENT_USER_STORE, constants.CAPICOM_MY_STORE, constants.CAPICOM_STORE_OPEN_MAXIMUM_ALLOWED);

            var result = action(store);

            yield store.Close();

            return result;
        });
    };

  var certCount = function() {
    return createObject("CAPICOM.Store").then(function(store) {
      return cadesplugin.async_spawn(function*() {
        yield store.Open(constants.CAPICOM_CURRENT_USER_STORE,
          constants.CAPICOM_MY_STORE,
          constants.CAPICOM_STORE_OPEN_MAXIMUM_ALLOWED
        );
        var cretificates = yield store.Certificates;

        return cretificates.Count;
      });
    });
  };

  var certById = function(id) {
    return createObject("CAPICOM.Store").then(function(store) {
      return cadesplugin.async_spawn(function*() {
        yield store.Open(constants.CAPICOM_CURRENT_USER_STORE,
          constants.CAPICOM_MY_STORE,
          constants.CAPICOM_STORE_OPEN_MAXIMUM_ALLOWED
        );

        var certificates = yield store.Certificates;
        var cert = yield certificates.Item(id + 1); //нумерация с 1 а не с 0 как обычно

          return getCertificateByPromise(cert);
      });
    });
    };

    var getCertificateByPromise = function(cert) {
        return cadesplugin.async_spawn(function*() {

            var subject = yield cert.SubjectName,
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
                organization = yield cert.GetInfo(constants.CAPICOM_INFO_SUBJECT_SIMPLE_NAME);
            }

            var role = "Не заказчик по 44-ФЗ"; //по умолчанию
            var isNotCustomer = true;

            var extendedUsage = yield cert.ExtendedKeyUsage();
            var coids = [];

            var ekus = yield extendedUsage.EKUs;
            var ekusCount = yield ekus.Count;
            for (var i = 1; i <= ekusCount ; i++) {
                var ekuItem = yield ekus.Item(i);
                var oid = yield ekuItem.OID;
                coids.push(oid);
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
                yield cert.ValidFromDate,
                yield cert.ValidToDate,
                names.join(' '),
                yield cert.GetInfo(constants.CAPICOM_INFO_ISSUER_SIMPLE_NAME),
                yield cert.Thumbprint,
                organization, role, isNotCustomer);
            return commonCert;
        });
    };
    var certByHash = function(certHash) {
        return cadesplugin.async_spawn(function*() {
            var store = yield createObject("CAPICOM.Store");

            yield store.Open(constants.CAPICOM_CURRENT_USER_STORE, constants.CAPICOM_MY_STORE, constants.CAPICOM_STORE_OPEN_MAXIMUM_ALLOWED);
            var certificates = yield store.Certificates;
            
            var oCertificates = yield certificates.Find(constants.CAPICOM_CERTIFICATE_FIND_SHA1_HASH, certHash);
            if (yield oCertificates.Count == 0) {
                throw new Error("Certificate not found");
            }
            
            var cert = yield oCertificates.Item(1);
            return Promise.resolve(getCertificateByPromise(cert));
        }).catch(function (error){console.log(error)});
    };
    var sign = function(content, contentEncoding, certificatePromise, isDetached) {
        return cadesplugin.async_spawn(function*() {
            var store = yield createObject("CAPICOM.Store");
            var certificate = yield certificatePromise;

            yield store.Open(constants.CAPICOM_CURRENT_USER_STORE, constants.CAPICOM_MY_STORE, constants.CAPICOM_STORE_OPEN_MAXIMUM_ALLOWED);

            var certificates = yield store.Certificates;
            var oCertificates = yield certificates.Find(constants.CAPICOM_CERTIFICATE_FIND_SHA1_HASH, certificate.Hash);

            if (yield oCertificates.Count == 0) {
                throw new Error("Certificate not found");
            }

            var oCertificate = yield oCertificates.Item(1);

            var signer = yield createObject("CAdESCOM.CPSigner");
            yield signer.propset_Certificate(oCertificate);

            var oSignedData = yield createObject("CAdESCOM.CadesSignedData");

            oSignedData.propset_ContentEncoding(contentEncoding);
            oSignedData.propset_Content(content);
            signer.propset_Options(constants.CAPICOM_CERTIFICATE_INCLUDE_END_ENTITY_ONLY);
            
            var sSignedMessage = yield oSignedData.SignCades(signer, constants.CADES_BES, isDetached);

            yield store.Close();

            return sSignedMessage;
        }).catch(function(error) {
            ShowError(error.message);
        });
    };

    var signBase64 = function(toSign, certificate, isDetached) {
        return sign(toSign, constants.CADESCOM_BASE64_TO_BINARY, certificate, isDetached);
    };

    var signString = function (toSign, certificate, isDetached) {
        return sign(toSign, constants.CADESCOM_STRING_TO_UCS2LE, certificate, isDetached);
    };


    var isCertHasGostAlgoritm = function(certificateHash) {
        return usedStore(function(store) {
            return cadesplugin.async_spawn(function*() {
                var certificates = yield store.Certificates;
                var oCertificates = yield certificates.Find(constants.CAPICOM_CERTIFICATE_FIND_SHA1_HASH, certificateHash);
                if (yield oCertificates.Count == 0) {
                    throw new Error("Certificate not found");
                }

                var cert = yield oCertificates.Item(1);
                var publicKey = yield cert.PublicKey();
                var algorithm = publicKey != null? yield  publicKey.Algorithm :null;
                var algorithmFriendlyName = algorithm != null? yield algorithm.FriendlyName: null;

                return Promise.resolve(
                    algorithmFriendlyName != null 
                    && (algorithmFriendlyName.toUpperCase().indexOf('ГОСТ') >= 0 || algorithmFriendlyName.toUpperCase().indexOf('GOST')) >= 0);
            });
        });
    };

    var getCertificates = function() {
            return cadesplugin.async_spawn(function*() {
                var store = yield createObject("CAPICOM.Store");
                yield store.Open(constants.CAPICOM_CURRENT_USER_STORE, constants.CAPICOM_MY_STORE, constants.CAPICOM_STORE_OPEN_MAXIMUM_ALLOWED);
                var certificates = yield store.Certificates;
                var certificateCount = yield certificates.Count;

                var certificatePromises = [];
                for (var index = 1; index <= certificateCount; index++) {
                    var cert = yield certificates.Item(index);
                    certificatePromises.push(getCertificateByPromise(cert));
                }
                store.Close();
                return Promise.all(certificatePromises);
            });
        };



        var signHash = function(hashToSign, currentCertificate) {
            return cadesplugin.async_spawn(function*() {
                var store = yield createObject("CAPICOM.Store");
                var certificate = yield currentCertificate;

                yield store.Open(constants.CAPICOM_CURRENT_USER_STORE, constants.CAPICOM_MY_STORE, constants.CAPICOM_STORE_OPEN_MAXIMUM_ALLOWED);

                var certificates = yield store.Certificates;
                var oCertificates = yield certificates.Find(constants.CAPICOM_CERTIFICATE_FIND_SHA1_HASH, certificate.Hash);

                if (yield oCertificates.Count == 0) {
                    throw new Error("Certificate not found");
                }

                var oCertificate = yield oCertificates.Item(1);

                // Создаем объект CAdESCOM.HashedData
                var cryptoProHashedData = yield createObject("CAdESCOM.HashedData");

                // Инициализируем объект заранее вычисленным хэш-значением
                // Алгоритм хэширования нужно указать до того, как будет передано хэш-значение
                cryptoProHashedData.propset_Algorithm(constants.CADESCOM_HASH_ALGORITHM_CP_GOST_3411);
                cryptoProHashedData.SetHashValue(hashToSign);

                var signer = yield createObject("CAdESCOM.CPSigner");
                yield signer.propset_Certificate(oCertificate);

                var oSignedData = yield createObject("CAdESCOM.CadesSignedData");

                signer.propset_Options(constants.CAPICOM_CERTIFICATE_INCLUDE_END_ENTITY_ONLY);
            
                var sSignedMessage = yield oSignedData.SignHash(cryptoProHashedData, signer, constants.CADES_BES);

                yield store.Close();

                return sSignedMessage;
            }).catch(function(error) {
                ShowError(error.message);
            });

        };


    var provider = {
        getCertificates: getCertificates,
        сertCount: certCount,
        certById: certById,
        certByHash: certByHash,
        signBase64: signBase64,
        signString: signString,
        signHash: signHash,
        isCertHasGostAlgoritm: isCertHasGostAlgoritm
    };

  return new Promise(function(resolve, reject) {
    resolve(provider);
  });
};

cryptoProviderFactory.asyncResolve();