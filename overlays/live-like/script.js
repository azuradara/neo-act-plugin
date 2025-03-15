const nf = new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }); 
const previousDPSValues = {};
const previousBarWidths = {};

layer.on('status', function (e) {
  if (e.type === 'lock') {
    e.message ? hideResizeHandle() : displayResizeHandle();
  }
});

function displayResizeHandle() {
  document.documentElement.classList.add("resizeHandle");
}

function hideResizeHandle() {
  document.documentElement.classList.remove("resizeHandle");
}

document.addEventListener('DOMContentLoaded', function () {
  layer.connect();
  layer.on('data', updateDPSMeter);
  setupZoomControls();
});

let popperInstance = null;

function animateDpsValue(element, start, end) {
  if (start === end) {
    element.textContent = `${nf.format(end)}/sec`;
    return;
  }

  const duration = 1000; 
  const startTime = Date.now();
  let lastValue = start;

  const update = () => {
    const elapsed = Date.now() - startTime;
    const progress = Math.min(elapsed / duration, 1);
    const currentValue = Math.floor(start + (end - start) * progress); 
    
    if (currentValue !== lastValue) {
      element.textContent = `${nf.format(currentValue)}/sec`;
      lastValue = currentValue;
    }

    if (progress < 1) {
      requestAnimationFrame(update);
    } else {
      element.textContent = `${nf.format(end)}/sec`;
    }
  };

  requestAnimationFrame(update);
}

function animateBarWidth(element, startPercent, endPercent) {
  const duration = 1000;
  const startTime = Date.now();

  const update = () => {
    const elapsed = Date.now() - startTime;
    const progress = Math.min(elapsed / duration, 1);
    const currentPercent = startPercent + (endPercent - startPercent) * progress;
    element.style.clipPath = `inset(0 ${100 - currentPercent}% 0 0)`;

    if (progress < 1) {
      requestAnimationFrame(update);
    } else {
      element.style.clipPath = `inset(0 ${100 - endPercent}% 0 0)`;
    }
  };

  requestAnimationFrame(update);
}

function updateDPSMeter(data) {
  document.getElementById('boss-name').innerText = data.Encounter.title || 'No Data';
  let table = document.getElementById('combatantTable');
  table.innerHTML = '';

  let combatants = Object.values(data.Combatant);
  combatants.sort((a, b) => parseFloat(b.encdps) - parseFloat(a.encdps));

  const maxEncdps = combatants.length > 0 
    ? Math.max(...combatants.map((c) => parseFloat(c.encdps) || 0)) 
    : 0;

  const currentNames = combatants.map(c => c.name);
  Object.keys(previousDPSValues).forEach(name => {
    if (!currentNames.includes(name)) delete previousDPSValues[name];
  });
  Object.keys(previousBarWidths).forEach(name => {
    if (!currentNames.includes(name)) delete previousBarWidths[name];
  });

  combatants.forEach((combatant) => {
    const currentDps = parseFloat(combatant.encdps) || 0;
    const widthPercentage = maxEncdps > 0 ? (currentDps / maxEncdps) * 100 : 0;

    let playerDiv = document.createElement('div');
    playerDiv.className = 'player' + (combatant.name === data.Combatant?.You?.name ? ' you' : '');
    playerDiv.setAttribute('data-player', combatant.name);

    let dpsBar = document.createElement('div');
    dpsBar.className = 'dps-bar';

    let gradientBg = document.createElement('div');
    gradientBg.className = 'gradient-bg';

    // Get previous width percentage
    const previousWidth = previousBarWidths[combatant.name] || 0;
    previousBarWidths[combatant.name] = widthPercentage;
    
    // Animate the bar width
    animateBarWidth(gradientBg, previousWidth, widthPercentage);

    let barContent = document.createElement('div');
    barContent.className = 'bar-content';

    const name = document.createElement('span');
    name.className = 'dps-bar-label';
    name.textContent = combatant.name;

    const dps = document.createElement('span');
    dps.className = 'dps-bar-value';
    
    const encdpsValue = combatant.encdps === '∞' ? 0 : Math.floor(parseFloat(combatant.encdps));
    const previousDps = previousDPSValues[combatant.name] !== undefined 
      ? previousDPSValues[combatant.name] 
      : encdpsValue;

    previousDPSValues[combatant.name] = encdpsValue;
    animateDpsValue(dps, previousDps, encdpsValue);

    barContent.appendChild(name);
    barContent.appendChild(dps);
    dpsBar.appendChild(gradientBg);
    dpsBar.appendChild(barContent);
    playerDiv.appendChild(dpsBar);
    table.appendChild(playerDiv);
  });
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
