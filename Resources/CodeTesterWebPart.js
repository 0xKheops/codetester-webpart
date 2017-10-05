
function CTWP_ToggleRefVisibility(blockId){
    var block = document.getElementById(blockId);
    if(block != null)
        block.style.display = (block.style.display != "block")?"block":"none";
}

function CTWP_ToggleVisibility(blockId1, blockId2, hfDisplayId){
    var block1 = document.getElementById(blockId1);
    if(block1 != null){
        block1.style.display = (block1.style.display == "none")?"block":"none";
        var block2 = document.getElementById(blockId2);
        if(block2 != null)
           block2.style.display = (block1.style.display == "block")?"none":"block";   
           
        var hfDisplay = document.getElementById(hfDisplayId);
        hfDisplay.value = block1.style.display;
    }
}