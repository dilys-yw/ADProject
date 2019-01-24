var inputNum = document.getElementsByTagName("input");
for (var i = 0; i < inputNum.length; i++) {
    inputNum[i].addEventListener("keyup", function () {
        this.value = this.value.replace(/\D/g, "");
        if (this.value.substring(this.value.toString().length - 1, 1) === /\D/g) {
            this.value = this.value.substring(0, this.value.length - 1);
        }
    });
}