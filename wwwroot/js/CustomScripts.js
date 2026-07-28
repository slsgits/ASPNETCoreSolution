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

function confirmUnlock(userId, isUnlockClicked) {

    var unlockSpan =
        document.getElementById('unlockSpan_' + userId);

    var confirmUnlockSpan =
        document.getElementById('confirmUnlockSpan_' + userId);

    if (isUnlockClicked) {
        unlockSpan.style.display = "none";
        confirmUnlockSpan.style.display = "inline";
    }
    else {
        unlockSpan.style.display = "inline";
        confirmUnlockSpan.style.display = "none";
    }
}