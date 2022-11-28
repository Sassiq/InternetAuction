var tabsPanel;
$(document).ready(function () {
    //Navigation Menu
    var trigger = $('.hamburger'),
     overlay = $('.overlay'),
    isClosed = false;
    
    trigger.click(function () {
        hamburger_cross();
    });

    function hamburger_cross() {

        if (isClosed == true) {
            overlay.hide();
            trigger.removeClass('is-open');
            trigger.addClass('is-closed');
            isClosed = false;
        } else {
            overlay.show();
            trigger.removeClass('is-closed');
            trigger.addClass('is-open');
            isClosed = true;
        }
    }

    $('[data-toggle="offcanvas"]').click(function () {
        $('#wrapper').toggleClass('toggled');
    });

 

    //Showing uploading images befor submit
    $("#uploadFile").on('change', function () {

        //Get count of selected files
        var countFiles = $(this)[0].files.length;

        var imgPath = $(this)[0].value;
        var extn = imgPath.substring(imgPath.lastIndexOf('.') + 1).toLowerCase();
        var image_holder = $("#ImageHolder");
        image_holder.empty();

        if (extn == "gif" || extn == "png" || extn == "jpg" || extn == "jpeg") {
            if (typeof (FileReader) != "undefined") {

                //loop for each file selected for uploaded.
                for (var i = 0; i < countFiles; i++) {

                    var reader = new FileReader();
                    reader.onload = function (e) {
                        $("<img />", {
                            "src": e.target.result,
                            "class": "thumb-image"
                        }).appendTo(image_holder);

                        $("#EditCatImg").hide();
                    }

                    image_holder.show();
                    reader.readAsDataURL($(this)[0].files[i]);
                }

            } else {
                alert("This browser does not support FileReader.");
            }
        } else {
            alert("Pls select only images");
        }
    });

    
   
    var allowInterval = false;
    tabsPanel = setInterval(function () {
            var tabs = $('.nav-tabs > li'),
                active = tabs.filter('.active'),
                next = active.next('li'),
                toClick = next.length ? next.find('a') : tabs.eq(0).find('a');
            toClick.trigger('click');
    }, 5000);
    
  
    $('.hamburger').click(function () {
        if (allowInterval == true) {
            tabsPanel = setInterval(function () {
                var tabs = $('.nav-tabs > li'),
                    active = tabs.filter('.active'),
                    next = active.next('li'),
                    toClick = next.length ? next.find('a') : tabs.eq(0).find('a');
                toClick.trigger('click');
            }, 5000);
            allowInterval = false;

        }
        else {
            window.clearInterval(tabsPanel);
            allowInterval = true;
        }
    });


    $("#btnBidsDetails").click(function () {
        $("#BidsDetailsPanel").slideToggle();
    });

    $("#btnAuctionDetails").click(function () {
        $("#AuctionDetailsPanel").slideToggle();
    });

    $("#btnAuctionOptions").click(function () {
        $("#AuctionOptionsPanel").slideToggle();
    });

    $("#btnRolesExplanation").click(function () {
        $("#RolesExplanationPanel").slideToggle();
    });

    
    $("#NoDescription").change(function () {
        if ($("#NoDescription").is(":checked")) {
            $("#Product_Description").val("No-Description");
            $("#Product_Description").attr("readonly", "readonly");
        }
        else {
            $("#Product_Description").val("");
            $("#Product_Description").removeAttr("readonly");
        }
    });

    var timezone = GetUserTimeZoneID();

});


$(window).load(function () {
    $(".loader-wrapper").fadeOut("slow");
});

var amountScrolled = 200;

$(window).scroll(function () {
    if ($(window).scrollTop() > amountScrolled) {
        $('a.backToTop').fadeIn('slow');
    } else {
        $('a.backToTop').fadeOut('slow');
    }
});

$('a.backToTop').click(function () {
    $('html, body').animate({
        scrollTop: 0
    }, 700);
    return false;
});

/*Image Modal*/
var modal = document.getElementById('ImgModal');

var img = document.getElementById('ProdImg');
var modalImg = document.getElementById("img");
var captionText = document.getElementById("caption");

/*img.onclick = function () {
    modal.style.display = "block";
    modalImg.src = this.src;
    modalImg.alt = this.alt;
    captionText.innerHTML = this.alt;
}*/

var span = document.getElementsByClassName("close")[0];

/*span.onclick = function () {
    modal.style.display = "none";
}*/

//Check bidding
function canBid() {
    var currentBid = $("#currentBid").val();
    var newBid = $("#Current_Bid").val();
    if (newBid <= currentBid) {
        $("#errorModal").modal();
        return false;
    }
}

//Checking if search keyword is empty
function keywordIsEmpty(){
    var k = $("#Keyword").val();
    if (k == null || k == "") {
        $("#AlertErrorSearch").show();
        return false;
    }
}

function CanExtend(){
    var days = $('#DurationDays').val();
    var hrs = $('#DurationHrs').val();

    if (days == null || days == "" || hrs == null || hrs == "") {
        $("#AlertErrorExtend").show();
        return false;
    }
}

function reportsEmpty() {
    if ($('#HdnReportsCount').val() == 0) {
        $('#errorModal').modal();
        return false;
    }
}

function GetUserTimeZoneID() {
    var timezone = String(new Date());
    return timezone.substring(timezone.lastIndexOf('(') + 1).replace(')', '').trim();
}
