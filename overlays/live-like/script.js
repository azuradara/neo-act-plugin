const nf = new Intl.NumberFormat('en-US')

document.addEventListener('DOMContentLoaded', function () {
  addOverlayListener('CombatData', updateDPSMeter)
  startOverlayEvents()
})

let popperInstance = null

function updateDPSMeter(data) {
  console.log(data)
  document.getElementById('boss-name').innerText = data.Encounter.title || 'No Data'

  let table = document.getElementById('combatantTable')
  table.innerHTML = ''

  let combatants = Object.values(data.Combatant)
  combatants.sort((a, b) => parseFloat(b.encdps) - parseFloat(a.encdps))

  const maxEncdps = combatants.length > 0 ? Math.max(...combatants.map((c) => parseFloat(c.encdps) || 0)) : 0

  combatants.forEach((combatant) => {
    const currentDps = parseFloat(combatant.encdps) || 0
    const widthPercentage = maxEncdps > 0 ? (currentDps / maxEncdps) * 100 : 0

    let playerDiv = document.createElement('div')
    playerDiv.className = 'player' + (combatant.name === data.Combatant?.You?.name ? ' you' : '')
    playerDiv.setAttribute('data-player', combatant.name)
    
    // playerDiv.addEventListener('mouseenter', (event) => showSkills(combatant, event))
    // playerDiv.addEventListener('mouseleave', hideSkills)

    let dpsBar = document.createElement('div')
    dpsBar.className = 'dps-bar'

    let gradientBg = document.createElement('div')
    gradientBg.className = 'gradient-bg'

    gradientBg.style.clipPath = `inset(0 ${100 - widthPercentage}% 0 0)`

    let barContent = document.createElement('div')
    barContent.className = 'bar-content'

    const name = document.createElement('span')
    name.className = 'dps-bar-label'
    name.textContent = combatant.name

    const dps = document.createElement('span')
    dps.className = 'dps-bar-value'
    console.log(combatant.ENCDPS)
    dps.textContent = `${nf.format(combatant.ENCDPS === '∞' ? 0 : combatant.ENCDPS)}/sec`

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
      <div class="skill-summary">Total Damage: ${combatant['damage-*']}</div>
      <div class="skill-labels">
          <span>Skill</span>
          <span>Hits</span>
          <span>Crit %</span>
          <span>Damage</span>
      </div>`

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

document.addEventListener('DOMContentLoaded', function () {
  addOverlayListener('CombatData', updateDPSMeter)
  startOverlayEvents()
})
