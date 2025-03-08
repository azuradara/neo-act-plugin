document.addEventListener("DOMContentLoaded", function () {
    addOverlayListener("CombatData", updateDPSMeter);
    startOverlayEvents();
  });
  
  function updateDPSMeter(data) {
    document.getElementById("boss-name").innerText =
      data.Encounter.title || "No Data";
    document.getElementById("global-dps").innerText = data.Encounter.encdps
      ? data.Encounter.encdps + " DPS"
      : "0 DPS";
  
    let table = document.getElementById("combatantTable");
    table.innerHTML = "";
  
    let combatants = Object.values(data.Combatant);
    combatants.sort((a, b) => parseFloat(b.encdps) - parseFloat(a.encdps)); // Sort by DPS descending
  
    combatants.forEach((combatant) => {
      let playerDiv = document.createElement("div");
      playerDiv.className = "player" + (combatant.name === data.You ? " you" : "");
      playerDiv.setAttribute("data-player", combatant.name);
      playerDiv.addEventListener("mouseover", (event) => showSkills(combatant, event));
      playerDiv.addEventListener("mouseleave", () => hideSkills());
  
      let dpsBar = document.createElement("div");
      dpsBar.className = "dps-bar";
      let span = document.createElement("span");
      span.style.width = combatant["damage%"] + "%";
      span.innerText = `${combatant.name} - ${combatant["damage-*"]} (${combatant.encdps} DPS) - ${combatant["damage%"]}%`;
      
      if (combatant.name === data.You) {
        span.style.color = "yellow"; // Highlight "You" in yellow
      }
      
      dpsBar.appendChild(span);
      playerDiv.appendChild(dpsBar);
      table.appendChild(playerDiv);
    });
  }
  
  function showSkills(combatant, event) {
    let skillDetails = document.getElementById("skill-details");
    let skillHTML = `
          <div class="skill-summary">Total Damage: ${combatant["damage-*"]}</div>
          <div class="skill-labels">
              <span>Skill</span>
              <span>Hits</span>
              <span>Crit %</span>
              <span>Damage</span>
          </div>`;
  
    let damageTypes = combatant.Items || [];
  
    if (damageTypes.length > 0) {
      damageTypes.forEach((damageType) => {
        damageType.Items.forEach((attack) => {
          skillHTML += `
                      <div class="skill">
                          <div class="skill-name">${attack.Type}</div>
                          <div class="skill-hits">${attack.Hits || 0}</div>
                          <div class="skill-crit">${attack.CritRate || 0}%</div>
                          <div class="skill-damage">${attack.Damage || 0}</div>
                      </div>`;
        });
      });
    } else {
      skillHTML += `<div class="skill">No skill data available</div>`;
    }
  
    skillDetails.innerHTML = skillHTML;
    skillDetails.style.display = "block";
    skillDetails.style.top = event.clientY + "px";
    skillDetails.style.left = event.clientX + "px";
  }
  
  function hideSkills() {
    document.getElementById("skill-details").style.display = "none";
  }
  
  document.addEventListener("DOMContentLoaded", function () {
    addOverlayListener("CombatData", updateDPSMeter);
    startOverlayEvents();
  });
  