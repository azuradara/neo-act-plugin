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

    if (combatant.name === 'Tamed') {
      gradientBg.classList.add('pink-gradient')
    }

    gradientBg.style.clipPath = `inset(0 ${100 - widthPercentage}% 0 0)`
    
    let barContent = document.createElement('div')
    barContent.className = 'bar-content'

    const name = document.createElement('span')
    name.className = 'dps-bar-label'
    
    if (combatant.name === 'renless') {
      name.innerHTML = combatant.name + ' <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 36 36" xml:space="preserve" style="width: 1rem; height: 1rem; vertical-align: middle;"><path fill="#77B255" d="m29.902 20.444-5.185-5.403a3.312 3.312 0 0 0-2.593-1.263H17.27a3.333 3.333 0 0 0-3.305 2.898L13 24l.015.13A7.4 7.4 0 0 0 13.263 36H25v-8.639c2.017-1.395 4.299-3.011 4.902-3.584 1.897-1.805 0-3.333 0-3.333zm-4.445 1.112v-.002.002z"/><path fill="#5B9239" d="m24.999 27.362-.001 1.309-3.94 2.674-2.641-4.139 5.645-5.81-1.939-1.923.501-.751 2.834 2.833-6.054 5.89 2.222 2.222s1.582-1.066 3.373-2.305zM17 22.582v-.928a8.357 8.357 0 0 0-1.442.232l.44-4.841-.996-.091-.478 5.262c-3.098 1.218-5.294 4.225-5.294 7.755 0 2.372.994 4.508 2.584 6.028h1.449a7.405 7.405 0 0 1-3.105-6.028c0-3.902 3.015-7.094 6.842-7.389z"/><path fill="#F9CA55" d="M20.331 27.532c-1.124-.535-2.19-.461-3.221.142-.493.289-.694.829.061 1.008.758.177 3.16-1.15 3.16-1.15z"/><path fill="#FFDC5D" d="M21.845 29.449c.139 1.765-2.226 3.414-4.02 3.199-1.282-.154-2.398-3.608-.877-4.053 1.356-.396 1.731-1.628 3.34-1.168 1.278.366 1.506 1.344 1.557 2.022z"/><path fill="#EF9645" d="m20.659 30.132-.212-.441s-.771 1.147-1.843 1.59c-.813.332-1.498.365-1.857.306.154.293.157.343.359.53 1.117.039 2.902-.56 3.553-1.985zm-2.217-.039c.99-.678 1.279-1.326 1.279-1.326l-.323-.409s-1.145 1.776-3.177 1.685c.015.248-.005.296.068.55.273.009 1.349.05 2.153-.5z"/><path fill="#FFDC5D" d="m11.647 6.297 1.832 4.41c1.008 2.424 2.382 4.16 4.803 3.165l.082.326c.206.817.055 1.478 1.258 1.524.841.032 1.977-1.47 1.771-2.287l-.41-1.636c1.203-1.435 1.921-3.269 1.304-4.751l-1.02-2.457c-5.514 2.076-7.783-.773-7.783-.773l-1.837 2.479z"/><path fill="#FFAC33" d="M20.927 1.747C18.323.301 15.458.154 12.312 2.87c-1.281 1.106-2.068 1.049-2.206 1.373-.527 1.964 1.81 3.868 2.267 3.317 1.446-1.744 3.265-1.84 3.998-1.154.706.677.852 2.248 1.184 2.122.9-.341.415-1.206.573-1.883.245-1.115 1.318-.978 1.866-.089.434.704.726 2.081-.36 2.798 1.146 1.606 2.453 1.527 2.453 1.527s.493.001 1.188-2.249.021-5.53-2.348-6.885z"/><path fill="#C1694F" d="m14.342 11.782-.308.128a.265.265 0 1 1-.204-.489l.309-.128a.265.265 0 1 1 .203.489z"/><path fill="#662113" d="M14.493 9.711a.53.53 0 0 1-.692-.286l-.205-.491a.53.53 0 0 1 .98-.407l.204.49a.53.53 0 0 1-.287.694z"/><path fill="#C1694F" d="M16.116 12.096c-.284.258-.724.622-1.266.989.189.216.39.402.604.558.511-.357.923-.704 1.2-.956a.4.4 0 1 0-.538-.591z"/></svg>'
    } else if (combatant.name === 'Eve') {
      name.innerHTML = combatant.name + ' <img src="./eve.svg" style="width: 1rem; height: 1rem; vertical-align: middle;" />'
    } else {
      name.textContent = combatant.name
    }

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
