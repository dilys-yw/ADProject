/*
*增加模板行
*/
function addRow() {
    var table = document.getElementsByName("addTable");
    var tbody = document.getElementById("templeteTBody");
    var newTBody = tbody.cloneNode(true);
    newTBody.style.display = "";
    var footTBody = document.getElementById("footTbody");
    return table[0].insertBefore(newTBody, footTBody);
}
/*
*删除模板行
*/

function deleteRow(obj) {
    var tbody = obj.parentNode.parentNode.parentNode;
    var table = document.getElementsByName("addTable");
    table[0].removeChild(tbody);
    numCheck();
}
/**
   *向模板中填充值
   */
function setValue() {
    var tbody = addRow();
    getInOrDe();
    numCheck();
    onlyNumInput();
}


function getInOrDe() {
    var btnDropdowns = document.getElementsByName("btnDropdowns");
    for (var i = 0; i < btnDropdowns.length; i++) {
        btnDropdowns[i].addEventListener("click", function () {
            var ddlmenu = this.nextElementSibling.nextElementSibling;
            ddlmenu.addEventListener("click", function (e) {
                //this.setAttribute("aria-labelledby",)
                //var $target = $(e.target);
                //$target.is('li') && $('#text').text($target.text());
                var $target = $(e.target);
                if ($target.is('li')) {
                    ddlmenu.parentElement.firstElementChild.firstElementChild.innerHTML = $target.html();
                }
            });
        });
    }

}
function numCheck() {
    var num = $("#templeteTBody tr").length;     //获取tr的长度
    for (var i = 1; i <= num; i++) {         //进行循环
        $("#templeteTBody tr .td_Num").eq(i).html(i);
    };
}
function onlyNumInput() {
    var inputNum = document.getElementsByName("qtyInput");
    for (var i = 0; i < inputNum.length; i++) {
        inputNum[i].addEventListener("keyup", function () {
            this.value = this.value.replace(/\D/g, "");
            if (this.value.substring(this.value.toString().length - 1, 1) === /\D/g) {
                this.value = this.value.substring(0, this.value.length - 1);
            }
        });
    }
}

function giveNum() {
    var num = $("#adjVoucherDetails tr").length;     //获取tr的长度
    for (var i = 0; i < num; i++) {         //进行循环
        $("#adjVoucherDetails tr .td_Num").eq(i).html(i+1);
    };
}