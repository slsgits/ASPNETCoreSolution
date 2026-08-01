// This file contains custom JavaScript functions for handling user interactions on the web page.

// Function to confirm deletion of a user
function confirmDelete(userId, isDeleteClicked) {

    // Get the delete and confirm delete span elements by their IDs
    var deleteSpan =
        document.getElementById('deleteSpan_' + userId);

    var confirmDeleteSpan =
        document.getElementById('confirmDeleteSpan_' + userId);

    // Toggle the display of the delete and confirm delete spans 
    // based on the isDeleteClicked parameter
    if (isDeleteClicked) {
        deleteSpan.style.display = "none";
        confirmDeleteSpan.style.display = "inline";
    }
    else {
        deleteSpan.style.display = "inline";
        confirmDeleteSpan.style.display = "none";
    }
}

// Function to confirm unlocking of a user
function confirmUnlock(userId, isUnlockClicked) {

    // Get the unlock and confirm unlock span elements by their IDs
    var unlockSpan =
        document.getElementById('unlockSpan_' + userId);

    var confirmUnlockSpan =
        document.getElementById('confirmUnlockSpan_' + userId);

    // Toggle the display of the unlock and confirm unlock spans
    if (isUnlockClicked) {
        unlockSpan.style.display = "none";
        confirmUnlockSpan.style.display = "inline";
    }
    else {
        unlockSpan.style.display = "inline";
        confirmUnlockSpan.style.display = "none";
    }
}