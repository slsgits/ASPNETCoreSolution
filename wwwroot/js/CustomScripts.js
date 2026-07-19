function confirmDelete(userId, isDeleteClicked) {

    var deleteSpan =
        document.getElementById('deleteSpan_' + userId);

    var confirmDeleteSpan =
        document.getElementById('confirmDeleteSpan_' + userId);

    if (isDeleteClicked) {
        deleteSpan.style.display = "none";
        confirmDeleteSpan.style.display = "inline";
    }
    else {
        deleteSpan.style.display = "inline";
        confirmDeleteSpan.style.display = "none";
    }
}