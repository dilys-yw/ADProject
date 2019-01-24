function validateRep() {
    var person = document.forms["repForm"]["repname"].value;
    if (person) {
        return true;
    }
    else {
        alert("You need to choose a representative.");
        return false;
    }
}


function validateAuthP() {
    var person = document.forms["authPForm"]["authperson"].value;
    var startdate = new Date(document.forms["authPForm"]["begindatep"].value);
    var enddate = new Date(document.forms["authPForm"]["enddatep"].value);
    var newdate = new Date();
    newdate.setHours(0);
    newdate.setMinutes(0);
    newdate.setMilliseconds(0);
    if (person && startdate && enddate) {
        {
            if (startdate.valueOf() >= newdate.valueOf() && enddate.valueOf() >= newdate.valueOf()) {
                {
                    if (startdate.valueOf() < enddate.valueOf()) {
                        return true;
                    }
                    else {
                        alert("Start date must not be later than end date.");
                        return false;
                    }
                }
            }
            else {
                alert("Assignment dates cannot be null or retrospective.");
                return false;
            }
        }
    }
    else {
        alert("All fields must be filled out");
        return false;
    }
}

function cancelAuthNRep() {
    if (!confirm("Are you sure you want to cancel the current person?")) {
        return false;
    } 


}