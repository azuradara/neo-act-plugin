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
      background: linear-gradient(to right, #000000 0%, #000000 15%, #0A0A0A 20%, #151515 30%, #252525 40%, #353535 50%, #454545 60%, #555555 70%, #454545 80%, #353535 90%, #252525 100%) !important;
      background-size: 200% 200% !important;
      animation: gradientFlow 6s ease infinite;
      opacity: 0.9;
    }
    .coca-cola-gradient {
      background: url('./cola-bg.jpg') !important;
      background-size: cover !important;
      background-position: center !important;
      opacity: 0.9;
    }
    .pink-gradient {
      background: linear-gradient(-45deg, #ff69b4, #ff1493, #ff007f, #db7093, #ffc0cb) !important;
      background-size: 200% 200% !important;
      animation: gradientFlow 6s ease infinite;
      opacity: 0.9;
    }
    .white-gradient {
      background: linear-gradient(-45deg, #ffffff, #f8f8f8, #f0f0f0, #e0e0e0, #c0c0c0) !important;
      background-size: 200% 200% !important;
      animation: gradientFlow 6s ease infinite;
      opacity: 0.9;
    }
    .panacea-gradient {
      background: #020024;
      background: linear-gradient(90deg, rgba(2, 0, 36, 1) 0%, rgba(47, 47, 196, 1) 66%, rgba(0, 212, 255, 1) 91%) !important;
      background-size: 200% 200% !important;
      animation: gradientFlow 6s ease infinite;
      opacity: 0.9;
    }
    .pepe-gradient {
      background: linear-gradient(-45deg, #8B4513, #A0522D, #8B4513, #654321, #5D4037) !important;
      background-size: 200% 200% !important;
      animation: gradientFlow 6s ease infinite;
      opacity: 0.9;
    }
    .eve-gradient {
      background: #2a0866fc;
      background: linear-gradient(194deg, rgba(42, 8, 102, 1) 0%, rgba(12, 79, 130, 1) 52%, rgba(31, 2, 2, 1) 98%) !important;
      background-size: 200% 200% !important;
      animation: gradientFlow 6s ease infinite;
      opacity: 0.9;
    }
    .whoah-gradient {
      background: #841617;
      background: radial-gradient(circle, rgba(132, 22, 23, 1) 0%, rgba(253, 29, 29, 1) 50%, rgba(48, 0, 0, 1) 100%) !important;
      background-size: 200% 200% !important;
      animation: gradientFlow 6s ease infinite;
      opacity: 0.9;
    }
    .renless-gradient {
      background: #000000;
      background: linear-gradient(32deg, rgba(0, 0, 0, 1) 0%, rgba(30, 92, 57, 1) 50%, rgba(169, 222, 55, 1) 100%) !important;
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

  setupZoomControls();
})

let popperInstance = null

function parseAnyNumberFormat(value) {
  if (value === undefined || value === null || value === '') {
    return 0;
  }
  
  if (typeof value === 'number') {
    return value;
  }
  
  if (value === '∞') {
    return 0;
  }
  
  const stringValue = String(value);
  
  if (stringValue.includes('.') && stringValue.includes(',')) {
    if (stringValue.lastIndexOf('.') < stringValue.lastIndexOf(',')) {
      return Number(stringValue.replace(/\./g, '').replace(',', '.'));
    } 
    else {
      return Number(stringValue.replace(/,/g, ''));
    }
  }
  
  if (stringValue.includes('.') && !stringValue.includes(',')) {
    if ((stringValue.match(/\./g) || []).length > 1) {
      return Number(stringValue.replace(/\./g, ''));
    }
    return Number(stringValue);
  }
  
  if (stringValue.includes(',') && !stringValue.includes('.')) {
    if ((stringValue.match(/,/g) || []).length > 1) {
      return Number(stringValue.replace(/,/g, ''));
    }
    return Number(stringValue.replace(',', '.'));
  }
  
  return Number(stringValue);
}

function updateDPSMeter(data) {
  console.log(data)
  document.getElementById('boss-name').innerText = data.Encounter.title || 'No Data'

  let table = document.getElementById('combatantTable')
  table.innerHTML = ''

  let combatants = Object.values(data.Combatant)
  
  combatants.forEach(combatant => {
    combatant.damageValue = parseAnyNumberFormat(combatant.damage);
    
    if (combatant.DPS !== undefined) {
      combatant.dpsValue = parseAnyNumberFormat(combatant.DPS);
    } else if (combatant.encdps !== undefined) {
      combatant.dpsValue = parseAnyNumberFormat(combatant.encdps);
    } else {
      combatant.dpsValue = 0;
    }
    
    if (combatant['damage%'] !== undefined) {
      const damagePercentStr = String(combatant['damage%']).replace('%', '');
      combatant.damagePercent = parseAnyNumberFormat(damagePercentStr);
    } else {
      combatant.damagePercent = 0;
    }
  })
  
  combatants.sort((a, b) => b.damageValue - a.damageValue)

  const maxDamage = combatants.length > 0 
    ? Math.max(...combatants.map(c => c.damageValue || 0)) 
    : 0

  combatants.forEach((combatant) => {
    const currentDamage = combatant.damageValue || 0
    const widthPercentage = maxDamage > 0 
      ? (currentDamage / maxDamage) * 100 
      : 0

    let playerDiv = document.createElement('div')
    
    playerDiv.setAttribute('data-player', combatant.name)
    // playerDiv.addEventListener('mouseenter', (event) => showSkills(combatant, event))
    // playerDiv.addEventListener('mouseleave', hideSkills)
    
    playerDiv.classList.add('player')

    const hasCustomGradient = 
      combatant.name === 'Shaddy' || 
      combatant.name === 'lll' || 
      combatant.name === 'hiya' || 
      combatant.name === 'K Z' || 
      combatant.name === 'Tamed' || 
      combatant.name === 'Panacea' || 
      combatant.name === 'NellanFM' ||
      combatant.name === 'Eve' ||
      combatant.name === 'Whoah' ||
      combatant.name === 'renless' ||
      combatant.name === 'Geaven' ||
      combatant.name === 'Coca Cola' ||
      combatant.name === 'neen';

    if ((combatant.name === 'You' || combatant.isSelf === 'true') && !hasCustomGradient) {
      playerDiv.classList.add('you')
    }

    let dpsBar = document.createElement('div')
    dpsBar.className = 'dps-bar'

    let gradientBg = document.createElement('div')
    gradientBg.className = 'gradient-bg'
    
    if (combatant.name === 'Shaddy') {
      gradientBg.classList.add('rgb-gradient')
    }

    if (combatant.name === 'lll' || combatant.name === 'hiya') {
      gradientBg.classList.add('black-gradient')
    }

    if (combatant.name === 'K Z') {
      gradientBg.classList.add('white-gradient')
    }

    if (combatant.name === 'Tamed') {
      gradientBg.classList.add('pink-gradient')
    }

    if (combatant.name === 'Panacea') {
      gradientBg.classList.add('panacea-gradient')
    }

    if (combatant.name === 'NellanFM') {
      gradientBg.classList.add('pepe-gradient')
    }

    if (combatant.name === 'Eve') {
      gradientBg.classList.add('eve-gradient')
    }

    if (combatant.name === 'Whoah') {
      gradientBg.classList.add('whoah-gradient')
    }

    if (combatant.name === 'renless') {
      gradientBg.classList.add('renless-gradient')
    }

    if (combatant.name === 'Geaven') {
      gradientBg.style.background = '#2980B9';
    }

    if (combatant.name === 'Coca Cola') {
      gradientBg.classList.add('coca-cola-gradient');
    }

    if (combatant.name === 'neen') {
      gradientBg.style.background = '#FF2134';
    }

    gradientBg.style.clipPath = `inset(0 ${100 - widthPercentage}% 0 0)`
    
    let barContent = document.createElement('div')
    barContent.className = 'bar-content'

    const name = document.createElement('span')
    name.className = 'dps-bar-label'
    
    if (combatant.name === 'renless') {
      name.innerHTML = combatant.name + ' <img src="./renless.png" style="width: 1rem; height: 1rem; vertical-align: middle;" />'
    } else if (combatant.name === 'Eve') {
      name.innerHTML = combatant.name + ' <img src="./eve.svg" style="width: 1rem; height: 1rem; vertical-align: middle;" />'
    } else if (combatant.name === 'NellanFM') {
      name.innerHTML = combatant.name + ' <img src="./nellan.webp" style="width: 1rem; height: 1rem; vertical-align: middle;" />'
    } else if (combatant.name === 'Feomatharia') {
      name.innerHTML = combatant.name + ' <img src="./wizard.png" style="width: 1rem; height: 1rem; vertical-align: middle;" />'
    } else if (combatant.name === 'Geaven') {
      name.innerHTML = combatant.name + ' <img src="./geaven.png" style="width: 1rem; height: 1rem; vertical-align: middle;" />'
    } else if (combatant.name === 'Coca Cola') {
      name.innerHTML = '<img src="./cola.png" style="height: 1rem; vertical-align: middle;" />'
    } else if (combatant.name === 'neen') {
      name.innerHTML = combatant.name + ' <img src="./neen.png" style="width: 1rem; height: 1rem; vertical-align: middle;" />'
    } else {
      name.textContent = combatant.name
    }

    const dps = document.createElement('span')
    dps.className = 'dps-bar-value'
    dps.textContent = `${nf.format(combatant.dpsValue)}/sec`

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
