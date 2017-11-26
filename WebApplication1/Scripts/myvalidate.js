function isnormal(str) {

    if (/^[A-Za-z\s]*$/.test(str)) {
        return true;
    }
    else {
        return false;
    }
}