const nf = new Intl.NumberFormat('en-US')

layer.on('status', function (e) {
  if (e.type === 'lock') {
    e.message ? hideResizeHandle() : displayResizeHandle();
  }
});

function displayResizeHandle() {
  document.documentElement.classList.add("resizeHandle")
}

function hideResizeHandle() {
  document.documentElement.classList.remove("resizeHandle")
}

document.addEventListener('DOMContentLoaded', function () {
  const q = new URLSearchParams(this.location.search);

  if (q.get('font') === 'kr') {
    document.documentElement.setAttribute('lang', 'kr')
  }

  const style = document.createElement('style');
  style.textContent = `
    .rgb-gradient {
      background: linear-gradient(-45deg, #ff0000, #ff8000, #ffff00, #00ff00, #00ffff, #0080ff, #0000ff, #8000ff, #ff00ff) !important;
      background-size: 200% 200% !important;
      animation: gradientFlow 6s ease infinite;
      opacity: 0.9;
    }
    .black-gradient {
      background: linear-gradient(to right, #000000 0%, #000000 15%, #101010 20%, #303030 30%, #707070 40%, #A0A0A0 50%, #D0D0D0 60%, #E8E8E8 70%, #FFFFFF 80%, #FFFFFF 90%, #FFFFFF 100%) !important;
      background-size: 200% 200% !important;
      animation: gradientFlow 6s ease infinite;
      opacity: 0.9;
    }
    .white-gradient {
      background: linear-gradient(-45deg, #ffffff, #f0f0f0, #e8e8e8, #d0d0d0) !important;
      background-size: 200% 200% !important;
      animation: gradientFlow 6s ease infinite;
      opacity: 0.9;
    }
    @keyframes gradientFlow {
      0% { background-position: 0% 50%; }
      25% { background-position: 100% 0%; }
      50% { background-position: 100% 100%; }
      75% { background-position: 0% 100%; }
      100% { background-position: 0% 50%; }
    }
  `;
  document.head.appendChild(style);

  layer.connect();
  layer.on('data', updateDPSMeter);

  setupZoomControls()
})

let popperInstance = null

function updateDPSMeter(data) {
  document.getElementById('boss-name').innerText = data.Encounter.title || 'No Data'

  let table = document.getElementById('combatantTable')
  table.innerHTML = ''

  let combatants = Object.values(data.Combatant)
  combatants.sort((a, b) => parseFloat(b['damage']) - parseFloat(a['damage']))

  const maxDamage = combatants.length > 0 
    ? Math.max(...combatants.map((c) => parseFloat(c['damage']) || 0)) 
    : 0

  combatants.forEach((combatant) => {
    const currentDamage = parseFloat(combatant['damage']) || 0
    const widthPercentage = maxDamage > 0 
      ? (currentDamage / maxDamage) * 100 
      : 0

    let playerDiv = document.createElement('div')
    
    playerDiv.setAttribute('data-player', combatant.name)
    playerDiv.addEventListener('mouseenter', (event) => showSkills(combatant, event))
    playerDiv.addEventListener('mouseleave', hideSkills)
    
    playerDiv.classList.add('player')

    if (combatant.name === 'You') {
      playerDiv.classList.add('you')
    }

    let dpsBar = document.createElement('div')
    dpsBar.className = 'dps-bar'

    let gradientBg = document.createElement('div')
    gradientBg.className = 'gradient-bg'
    
    if (combatant.name === 'Shaddy') {
      gradientBg.classList.add('rgb-gradient')
    }

    if (combatant.name === 'lll') {
      gradientBg.classList.add('black-gradient')
    }

    if (combatant.name === 'K Z') {
      gradientBg.classList.add('white-gradient')
    }

    gradientBg.style.clipPath = `inset(0 ${100 - widthPercentage}% 0 0)`
    
    let barContent = document.createElement('div')
    barContent.className = 'bar-content'

    const name = document.createElement('span')
    name.className = 'dps-bar-label'
    name.textContent = combatant.name

    const dps = document.createElement('span')
    dps.className = 'dps-bar-value'
    dps.textContent = `${nf.format(combatant.DPS === '∞' ? 0 : combatant.DPS)}/sec`

    barContent.appendChild(name)
    barContent.appendChild(dps)
    dpsBar.appendChild(gradientBg)
    dpsBar.appendChild(barContent)
    playerDiv.appendChild(dpsBar)
    table.appendChild(playerDiv)
  })
}

function showSkills(combatant, event) {
  const skillDetails = document.getElementById('skill-details')
  const referenceElement = {
    getBoundingClientRect: () => ({
      width: 0,
      height: 0,
      top: event.clientY,
      right: event.clientX,
      bottom: event.clientY,
      left: event.clientX,
    }),
  }

  let skillHTML = `
      <div class="skill-summary">Total Damage: ${combatant['damage-*']} (${combatant['damage%']})</div>
      <div class="skill-summary">Hits: ${combatant['hits']}</div>
      <div class="skill-summary">Total Crit %: ${combatant['crithit%']}</div>
      <div class="skill-summary">Max Hit: ${combatant['maxhit-*']}</div>
      <div class="skill-labels">
          <span>Skill</span>
          <span>Hits</span>
          <span>Crit %</span>
          <span>Damage</span>
      </div>`
      
  /* TODO: Add skill details and stats for them.
  let damageTypes = combatant.Items || []

  if (damageTypes.length > 0) {
    damageTypes.forEach((damageType) => {
      damageType.Items.forEach((attack) => {
        skillHTML += `
                      <div class="skill">
                          <div class="skill-name">${attack.Type}</div>
                          <div class="skill-hits">${attack.Hits || 0}</div>
                          <div class="skill-crit">${attack.CritRate || 0}%</div>
                          <div class="skill-damage">${attack.Damage || 0}</div>
                      </div>`
      })
    })
  } else {
    skillHTML += `<div class="skill">No skill data available</div>`
  }
  */

  skillHTML += `<div class="skill">No skill data available</div>`
  skillDetails.innerHTML = skillHTML
  skillDetails.style.display = 'block'

  if (popperInstance) {
    popperInstance.destroy()
  }

  popperInstance = Popper.createPopper(referenceElement, skillDetails, {
    placement: 'right-start',
    modifiers: [
      {
        name: 'offset',
        options: {
          offset: [0, 10],
        },
      },
      {
        name: 'preventOverflow',
        options: {
          padding: 10,
        },
      },
      {
        name: 'flip',
        options: {
          padding: 10,
        },
      },
    ],
  })
}

function hideSkills() {
  const skillDetails = document.getElementById('skill-details')
  skillDetails.style.display = 'none'
  if (popperInstance) {
    popperInstance.destroy()
    popperInstance = null
  }
}

function setupZoomControls() {
  const zoomOutBtn = document.getElementById('zoom-out');
  const zoomInBtn = document.getElementById('zoom-in');
  const root = document.documentElement;

  let currentZoom = 100; 
  const minZoom = 50;
  const maxZoom = 200;
  const zoomStep = 10;

  const savedZoom = localStorage.getItem('dpsMeterZoom');
  if (savedZoom) {
    currentZoom = parseInt(savedZoom);
    applyZoom();
  }

  function applyZoom() {
    root.style.fontSize = `${currentZoom / 100}rem`;
    
    localStorage.setItem('dpsMeterZoom', currentZoom);
  }

  zoomOutBtn.addEventListener('click', () => {
    currentZoom = Math.max(minZoom, currentZoom - zoomStep);
    applyZoom();
  });

  zoomInBtn.addEventListener('click', () => {
    currentZoom = Math.min(maxZoom, currentZoom + zoomStep);
    applyZoom();
  });

  document.querySelectorAll('.zoom-btn').forEach(element => {
    element.addEventListener('mousedown', (e) => {
      e.stopPropagation();
      e.preventDefault();
    });
  });
}

document.removeEventListener('DOMContentLoaded', setupZoomControls);
