$(document).ready(function() {
    $('.tabs__nav a').click(function () {
        setActiveTab($(this));
    });
    
    var setActiveTab = function(tab){
        $('.tabs__nav li').removeClass('active');
        tab.parent().addClass('active');

        var currentTab = tab.attr('href');
        $('.tabs__content').hide();
        $(currentTab).show();
        return false;
    }

    setActiveTab($(".credentialsTab"));

    $('#certListId').on("click", "li", function () {
        $(".login-form__input").val("");
    });

    $(".certificateTab").click(function () {
        initiateCertificateList();
    });
});