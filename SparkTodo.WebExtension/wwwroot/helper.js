function toggleElement(id) {
    let element = document.getElementById(id);
    if (element){
        element.style.display = element.style.display==='none'?'block':'none';
    }
}