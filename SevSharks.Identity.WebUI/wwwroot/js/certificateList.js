var initiateCertificateList = function () {
    var certListId = $('#certListId');
    $(certListId).empty();

    //Если пользователь кликает по строке, то строка выделяется серым цветом, как в примере «Петров Петр Петрович».
    $(certListId).on("click", "li", function() {

        var self = $(this);

        self.closest("ul").find("li.selected").toggleClass("selected");
        self.addClass("selected");

    });

    //- Если набраны логин и пароль и после этого пользователь выбирает сертификат, то поля «Логин» и «Пароль» очищаются на вариант по умолчанию.
    $(certListId).on("click", "li", function() {

        var self = $(this);
        $(".login-form__input").val("");
    });


    $(certListId).on("click", "li", function() {

        $("#submitSignin").attr('disabled', false);

    });

    $(certListId).on("dblclick", "li", function () {

        var self = $(this);
        self.closest("ul").find("li.selected").toggleClass("selected");
        self.addClass("selected");
        $('#submitSignin').click();
    });

    $('#submitSignin').click(function (e) {
        e.preventDefault();
        $('#SignedData').val(null);
        if ($('#certListId .selected').length > 0) {
            var certHash = $('#certListId .selected').attr('data-hash');
            var objectsForSign = {};
            objectsForSign["formDataForSign"] = {
                data: dateDateForSign,
                isDetached: true
            };
            dataSignHelper.showLoader = false;
            dataSignHelper.signObjects(objectsForSign, function (signatures) {
                $('#SignedData').val(signatures["formDataForSign"].signature);
                sendPost(e);
            }, false, true, certHash);
        }
    });

    var certListItem = certListId.children(); 

    $(function () {
        cryptoHelper.getCertList().then(function (certList) {
            if (!certList.length) {
                $('#certListEmptyModel').arcticmodal();
                return;
            }

            $(certList).each(function () {
                $('#certListModel').arcticmodal();
                var certrow = $('<li style="' + (this.IsValid() ? '' : 'display:none') + '" data-isValid="' + this.IsValid() + '" data-hash="' + this.Hash + '" class="sert-list__item"><label class="sert-list__item-label"><p class="sert-list__input-wrapper"><input type="radio" name="serts" checked><span class="sert-list__item-radio"></span></p>' +
                    '<p class="sert-list__item-titles"><strong class="sert-list__item-name">' + this.Subject + '</strong><span class="sert-list__item-company">' + this.Organization + '</span></p><p class="sert-list__item-date">' + this.CertificateValidationPeriod() + '</p></label></li>');

                $(certListId).each(function () {
                    $(this).append(certrow);
                });
            });

            var shownotactivecerts = false;
            $('#show-noatactive-certs').click(function () {

                $(certListItem).removeClass('selected');
                $('#submitSignin').attr('disabled', 'disabled');
                shownotactivecerts = !shownotactivecerts;
                if (shownotactivecerts) {
                    $('#certListId li[data-isValid=false]').show();
                } else {
                    $('#certListId li[data-isValid=false]').hide();
                }
            });
            initOnEnterClick('#certListId li[data-hash]', '#submitSignin');
        }).then(null, function (error) { console.log(error) });
    });

};
function initOnEnterClick(inputSelector, buttonSelector) {
    $(inputSelector).keyup(function (event) {
        if (event.keyCode == 13) {
            if ($(buttonSelector).attr('disabled') != 'disabled') {
                $(buttonSelector).click();
            }
        }
    });
}

var cryptoHelper = {

    //открытие окна с выбором сертификата для подписи
    //selectCertificateHandler - обработчик выбора сертификата
    oids: {
        id_eku_GF05_authorized: { oid: '1.2.643.3.61.502710.1.6.3.4.2', name: 'Уполномоченный орган' },
        id_eku_GF05_authorizedInstitute: { oid: '1.2.643.3.61.502710.1.6.3.4.16', name: 'Уполномоченное учреждение' },
        id_eku_GF05_customer: { oid: '1.2.643.3.61.502710.1.6.3.4.1', name: 'Заказчик' },
        id_eku_GF05_specializedOrg: { oid: '1.2.643.3.61.502710.1.6.3.4.3', name: 'Специализированная организация' },
        id_eku_GF05_supervising: { oid: '1.2.643.3.61.502710.1.6.3.4.4', name: 'Контролирующий орган' },
        id_eku_GF05: { oid: '1.2.643.3.61.502710.1.6.3.4', name: 'Прочий' }
    },
    getCertList: function () {
        var cryptoPro = window.cryptoProviderFactory.create();
        return cryptoPro.then(function (cryptoProvider) {
            return Promise.all([Promise.resolve(cryptoProvider), cryptoProvider.сertCount()]);
        }).then(function (cryptoProviderWithCertCount) {
            var cryptoProvider = cryptoProviderWithCertCount[0];
            var certCount = cryptoProviderWithCertCount[1];

            //получаем список сертификатов
            var certList = [];
            for (var i = 0; i < certCount; i++) {
                certList.push(cryptoProvider.certById(i));
            }
            return Promise.all(certList);
        }).then(function(certificates) {
            certificates.sort(function (a, b) { return (a.Subject > b.Subject) - (a.Subject < b.Subject) });
            return Promise.resolve(certificates);
        }).then(null, function (error) { console.log(error) });
    },
    isStartsWithOid: function (oids, oid) {
        if (oids.length === 0) {
            return false;
        }
        for (var i = 0; i < oids.length; i++) {
            if (oids[i].indexOf(oid) === 0) {
                return true;
            }
        }
        return false;
    },

    _getSelectedCertificate: function () {

        var $liHash = $("#digital-certs").data("kendoGrid").select().find("li[data-name='hash']");
        if (!$liHash)
            return null;
        return $liHash.html();
    }



};
var signingF;
dataSignHelper = {
    showLoader: true,
    signObjects: function (objectsToSign, successCallback, useCachedCert, saveCertInCache, certHash, cancelCallback, errorSignCallback) {
        var signingF = function (hash, validateSignature) {
            var signatures = dataSignHelper._signProcess(objectsToSign, hash, validateSignature, errorSignCallback);
            //ошибка подписи
            if (signatures == null) {
                if (cancelCallback != undefined) {
                    cancelCallback();
                }
                return;
            }

            signatures.then(function (_signatures) {
                successCallback(_signatures);
            });
        };

        if (certHash) {
            signingF(certHash, true);
            return;
        }
        if (typeof useCachedCert === 'undefined' || useCachedCert) {
            signingF(loggedInCertHash);
            return false;
        }
    },

    _signProcess: function (objectsToSign, certHash, validateSignature, errorSignCallback) {
        try {
            if (dataSignHelper.showLoader == true) {
                commonHelper.kendoProgressStart();
            }

            var cryptoPro = window.cryptoProviderFactory.create();
            return cryptoPro.then(function (cryptoProvider) {
                return Promise.all([Promise.resolve(cryptoProvider), cryptoProvider.certByHash(certHash)]);
            }).then(function (cryptoProviderWithCert) {
                var signatures = {};
                var signaturesPromise = [];
                var cryptoProvider = cryptoProviderWithCert[0];
                var cert = Promise.resolve(cryptoProviderWithCert[1]);

                $.each(objectsToSign, function (index, value) {
                    signatures[index] = {};
                    signatures[index].data = value.data;
                    signatures[index].isDetached = value.isDetached;
                    if (value.isHash) {
                        try {
                            signaturesPromise.push(cryptoProvider.signHash(value.data, cert));
                        } catch (ex) {
                        }
                    } else {
                        signaturesPromise.push(cryptoProvider.signBase64(value.data, cert, value.isDetached));
                    }
                });

                return Promise.all(signaturesPromise).then(function (signs) {

                    var currentPosition = 0;
                    $.each(objectsToSign, function (key) {
                        signatures[key].signature = signs[currentPosition];
                        currentPosition++;
                    });
                    return signatures;
                });
            }).then(function (signatures) {
                if (validateSignature) {
                    var errorMessage = '';
                    for (signature in signatures) {
                        $.ajax({
                            url: validateSignatureURL,
                            type: 'POST',
                            async: false,
                            data: { signature: signatures[signature].signature, bytesToSign: signatures[signature].data, isDetached: signatures[signature].isDetached },
                            cache: false,
                            error: function (x, t, m) {
                                errorMessage = m;
                            },
                            success: function (data) {
                                if (data.signError != null) {
                                    errorMessage = data.signError;
                                }
                            }
                        });
                        break;
                    }
                }
                return signatures;

            }).then(null, function (error) {
                $('#errorMessage').html('В процессе подписи произошла ошибка: ' + error);
            });
        } catch (err) {
            $('#errorMessage').html('В процессе подписи произошла ошибка: ' + err.message);
            return null;
        }
    }
};

function sendPost(e) {
    $.ajax({
        url: sendPostURL,
        type: "POST",
        data: {
            'SignedData': $('#SignedData').val(),
            'ReturnUrl': $('#ReturnUrl').val()
        },
        dataType: "json",
        traditional: true,
        success: function (data) {
            location.href = data.url;
        },
        error: function () {
            $("#tab1 .validation__summary .form__group-validation").text('Ошибка выбора сертификата');
            $("#tab1 .validation__summary").show();
        }
    });
}

function CreateCrypto() {
    var cryptoProvider;
    if (RtsCrypto.IsCorrectVersionInstalled()) {
        cryptoProvider = new RtsCrypto();
    } else {
        cryptoProvider = window.cryptoProviderFactory.create();
    }
    return cryptoProvider;
}