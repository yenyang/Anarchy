// Function to set up Anarchy Button
if (typeof yyAnarchy.setupAnarchyItem !== 'function')
{
    yyAnarchy.setupAnarchyItem = function (selected)
    {
        const button = document.getElementById("YYA-Anarchy-Button");
        if (button == null) {
            engine.trigger('YYA-log', "JS Error: could not setup button YYA-Anarchy-Button");
            return;
        }
        if (selected) {
            button.classList.add("selected");
            const image = document.getElementById("YYA-Anarchy-Image");
            if (image != null) {
                image.src = "coui://uil/Colored/Anarchy.svg";
            }
            if (yyAnarchy.flamingChirper) {
                var y = document.getElementsByTagName("img");
                for (let i = 0; i < y.length; i++) {
                    if (y[i].src == "coui://GameUI/Media/Game/Icons/Chirper.svg" || y[i].src == "Media/Game/Icons/Chirper.svg") y[i].src = "coui://uil/Colored/AnarchyChirper.svg";
                }
            }
        } else {
            const image = document.getElementById("YYA-Anarchy-Image");
            if (image != null) {
                image.src = "coui://uil/Standard/Anarchy.svg";
            }
            button.classList.remove("selected");
            var y = document.getElementsByTagName("img");
            for (let i = 0; i < y.length; i++) {
                if (y[i].src == "coui://uil/Colored/AnarchyChirper.svg") y[i].src = "coui://GameUI/Media/Game/Icons/Chirper.svg";
            }
        }
        button.onclick = function () {
            let selected = true;
            if (this.classList.contains("selected")) {
                selected = false; // This is intended to toggle and be the opposite of what it is now.
            }
            engine.trigger('YYA-AnarchyToggled', selected);
            const thisButton = document.getElementById(this.id);
            if (selected) {
                thisButton.classList.add("selected");
                const image = document.getElementById("YYA-Anarchy-Image");
                if (image != null) {
                    image.src = "coui://uil/Colored/Anarchy.svg";
                }
                if (yyAnarchy.flamingChirper) {
                    var y = document.getElementsByTagName("img");
                    for (let i = 0; i < y.length; i++) {
                        if (y[i].src == "coui://GameUI/Media/Game/Icons/Chirper.svg" || y[i].src == "Media/Game/Icons/Chirper.svg") y[i].src = "coui://uil/Colored/AnarchyChirper.svg";
                    }
                }
            } else {
                const image = document.getElementById("YYA-Anarchy-Image");
                if (image != null) {
                    image.src = "coui://uil/Standard/Anarchy.svg";
                }
                thisButton.classList.remove("selected");
                var y = document.getElementsByTagName("img");
                for (let i = 0; i < y.length; i++) {
                    if (y[i].src == "coui://uil/Colored/AnarchyChirper.svg") y[i].src = "coui://GameUI/Media/Game/Icons/Chirper.svg";
                }
            }   
        }
        yyAnarchy.setTooltip("YYA-Anarchy-Button", "AnarchyButton");
    }
}

if (typeof yyAnarchy.CheckForElementByID !== 'function') {
    yyAnarchy.CheckForElementByID = function (id)
    {
        if (document.getElementById(id) != null) {
            engine.trigger('CheckForElement-'+id , true);
            return;
        }
        engine.trigger('CheckForElement-' + id, false);
    }
}

// Function to apply translation strings.
if (typeof yyAnarchy.applyLocalization !== 'function') {
    yyAnarchy.applyLocalization = function (target) {
        if (!target) {
            return;
        }

        let targets = target.querySelectorAll('[localeKey]');
        targets.forEach(function (currentValue) {
            currentValue.innerHTML = engine.translate(currentValue.getAttribute("localeKey"));
        });
    }
}

// Function to setup tooltip.
if (typeof yyAnarchy.setTooltip !== 'function') {
    yyAnarchy.setTooltip = function (id, toolTipKey) {
        let target = document.getElementById(id);
        target.onmouseenter = () => yyAnarchy.showTooltip(document.getElementById(id), toolTipKey);
        target.onmouseleave = yyAnarchy.hideTooltip;
    }
}

// Function to show a tooltip, creating if necessary.
if (typeof yyAnarchy.showTooltip !== 'function') {
    yyAnarchy.showTooltip = function (parent, tooltipKey) {

        if (!document.getElementById("yyAnarchyToolip")) {
            yyAnarchy.tooltip = document.createElement("div");
            yyAnarchy.tooltip.id = "yyAnarchyToolip";
            yyAnarchy.tooltip.style.visibility = "hidden";
            yyAnarchy.tooltip.classList.add("balloon_qJY", "balloon_H23", "up_ehW", "center_hug", "anchored-balloon_AYp", "up_el0");
            let boundsDiv = document.createElement("div");
            boundsDiv.classList.add("bounds__AO");
            let containerDiv = document.createElement("div");
            containerDiv.classList.add("container_zgM", "container_jfe");
            let contentDiv = document.createElement("div");
            contentDiv.classList.add("content_A82", "content_JQV");
            let arrowDiv = document.createElement("div");
            arrowDiv.classList.add("arrow_SVb", "arrow_Xfn");
            let broadDiv = document.createElement("div");
            yyAnarchy.tooltipTitle = document.createElement("div");
            yyAnarchy.tooltipTitle.classList.add("title_lCJ");
            let paraDiv = document.createElement("div");
            paraDiv.classList.add("paragraphs_nbD", "description_dNa");
            yyAnarchy.tooltipPara = document.createElement("p");
            yyAnarchy.tooltipPara.setAttribute("cohinline", "cohinline");

            paraDiv.appendChild(yyAnarchy.tooltipPara);
            broadDiv.appendChild(yyAnarchy.tooltipTitle);
            broadDiv.appendChild(paraDiv);
            containerDiv.appendChild(arrowDiv);
            contentDiv.appendChild(broadDiv);
            boundsDiv.appendChild(containerDiv);
            boundsDiv.appendChild(contentDiv);
            yyAnarchy.tooltip.appendChild(boundsDiv);

            // Append tooltip to screen element.
            let screenParent = document.getElementsByClassName("game-main-screen_TRK");
            if (screenParent.length == 0) {
                screenParent = document.getElementsByClassName("editor-main-screen_m89");
            }
            if (screenParent.length > 0) {
                screenParent[0].appendChild(yyAnarchy.tooltip);
            }
        }

        // Set text and position.
        yyAnarchy.tooltipTitle.innerHTML = engine.translate("YY_ANARCHY." + tooltipKey);
        yyAnarchy.tooltipPara.innerHTML = engine.translate("YY_ANARCHY_DESCRIPTION." + tooltipKey);

        // Set visibility tracking to prevent race conditions with popup delay.
        yyAnarchy.tooltipVisibility = "visible";

        // Slightly delay popup by three frames to prevent premature activation and to ensure layout is ready.
        window.requestAnimationFrame(() => {
            window.requestAnimationFrame(() => {
                window.requestAnimationFrame(() => {
                    yyAnarchy.setTooltipPos(parent);
                });

            });
        });
    }
}

// Function to adjust the position of a tooltip and make visible.
if (typeof yyAnarchy.setTooltipPos !== 'function') {
    yyAnarchy.setTooltipPos = function (parent) {
        if (!yyAnarchy.tooltip) {
            return;
        }

        let tooltipRect = yyAnarchy.tooltip.getBoundingClientRect();
        let parentRect = parent.getBoundingClientRect();
        let xPos = parentRect.left + ((parentRect.width - tooltipRect.width) / 2);
        let yPos = parentRect.top - tooltipRect.height;
        yyAnarchy.tooltip.setAttribute("style", "left:" + xPos + "px; top: " + yPos + "px; --posY: " + yPos + "px; --posX:" + xPos + "px");

        yyAnarchy.tooltip.style.visibility = yyAnarchy.tooltipVisibility;
    }
}

// Function to hide the tooltip.
if (typeof yyAnarchy.hideTooltip !== 'function') {
    yyAnarchy.hideTooltip = function () {
        if (yyAnarchy.tooltip) {
            yyAnarchy.tooltipVisibility = "hidden";
            yyAnarchy.tooltip.style.visibility = "hidden";
        }
    }
}