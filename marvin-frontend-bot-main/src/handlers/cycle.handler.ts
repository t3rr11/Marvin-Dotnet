const resetTime = '17:00 UTC';
const msPerWk = 604800000;
const msPerDay = 86400000;

const cycleInfo = {
  epoch: {
    zeroHour: new Date(`May 7 2019 ${resetTime}`).getTime(),
    lostSector: new Date(`May 24 2022 ${resetTime}`).getTime(),
    grandMaster: new Date(`December 22 2020 ${resetTime}`).getTime(),
    wellSpring: new Date(`June 6 2022 ${resetTime}`).getTime(),
  },
  cycle: {
    zeroHour: 3,
    lostSector: 44,
    grandMaster: 6,
    wellSpring: 4,
  },
  elapsed: {
    zeroHour: undefined,
    lostSector: undefined,
    grandMaster: undefined,
    wellSpring: undefined,
  },
  week: {
    zeroHour: undefined,
    lostSector: undefined,
    grandMaster: undefined,
    wellSpring: undefined,
  },
  day: {
    zeroHour: undefined,
    lostSector: undefined,
    grandMaster: undefined,
    wellSpring: undefined,
  },
};

const getLoot = (type) => {
  switch (type) {
    case 0: {
      return { type: 'Chest', loot: [] };
    }
    case 1: {
      return { type: 'Helmet', loot: [] };
    }
    case 2: {
      return { type: 'Legs', loot: [] };
    }
    case 3: {
      return { type: 'Arms', loot: [] };
    }
  }
};

const getSector = (type) => {
  switch (type) {
    case 0: {
      return { name: 'K1 Crew Quarters', masterHash: 184186578, legendHash: 184186581, planet: 'Moon' };
    }
    case 1: {
      return { name: 'K1 Logistics', masterHash: 567131519, legendHash: 567131512, planet: 'Moon' };
    }
    case 2: {
      return { name: 'K1 Revelations', masterHash: 3911969238, legendHash: 3911969233, planet: 'Moon' };
    }
    case 3: {
      return { name: 'K1 Communion', masterHash: 2829206720, legendHash: 2829206727, planet: 'Moon' };
    }
    case 4: {
      return { name: 'The Conflux', masterHash: 1163502296, legendHash: 1163502303, planet: 'Nessus' };
    }
    case 5: {
      return { name: 'Metamorphosis', masterHash: 3678847134, legendHash: 3678847129, planet: 'Throne World' };
    }
    case 6: {
      return { name: 'Sepulcher', masterHash: 480864721, legendHash: 480864726, planet: 'Throne World' };
    }
    case 7: {
      return { name: 'Extraction', masterHash: 145221020, legendHash: 145221019, planet: 'Throne World' };
    }
    case 8: {
      return { name: 'Excavation Site XII', masterHash: 548616653, legendHash: 548616650, planet: 'EDZ' };
    }
    case 9: {
      return { name: 'Skydock IV', masterHash: 55186256, legendHash: 55186263, planet: 'EDZ' };
    }
    case 10: {
      return { name: 'The Quarry', masterHash: 3253890600, legendHash: 3253890607, planet: 'EDZ' };
    }

    // case 0: { return { name: "Aphelion's Rest", masterHash: 1898610131, legendHash: 1898610132, planet: "The Dreaming City" } }
    // case 1: { return { name: "Bay of Drowned Wishes", masterHash: 660710120, legendHash: 660710127, planet: "The Dreaming City" } }
    // case 2: { return { name: "Chamber of Starlight", masterHash: 4206916276, legendHash: 4206916275, planet: "The Dreaming City" } }
    // case 9: { return { name: "Veles Labyrinth", masterHash: 3094493727, legendHash: 3094493720, planet: "Cosmodrome" } }
    // case 10: { return { name: "Exodus Garden 2A", masterHash: 2936791995, legendHash: 2936791996, planet: "Cosmodrome" } }
    // case 3: { return { name: "The Empty Tank", masterHash: 2019961993, legendHash: 2019961998, planet: "Tangled Shore" } }
    // case 8: { return { name: "Consealed Void", masterHash: 912873274, legendHash: 912873277, planet: "Europa" } }
    // case 9: { return { name: "Bunker E15", masterHash: 1648125538, legendHash: 1648125541, planet: "Europa" } }
    // case 10: { return { name: "Perdition", masterHash: 1070981425, legendHash: 1070981430, planet: "Europa" } }
    // case 7: { return { name: "Scavengers Den", masterHash: 1905792146, legendHash: 1905792149, planet: "EDZ" } }
  }
};

const rotations = {
  lostSector: {
    0: { sector: getSector(0), loot: getLoot(0) },
    1: { sector: getSector(1), loot: getLoot(1) },
    2: { sector: getSector(2), loot: getLoot(2) },
    3: { sector: getSector(3), loot: getLoot(3) },
    4: { sector: getSector(4), loot: getLoot(0) },
    5: { sector: getSector(5), loot: getLoot(1) },
    6: { sector: getSector(6), loot: getLoot(2) },
    7: { sector: getSector(7), loot: getLoot(3) },
    8: { sector: getSector(8), loot: getLoot(0) },
    9: { sector: getSector(9), loot: getLoot(1) },
    10: { sector: getSector(10), loot: getLoot(2) },

    11: { sector: getSector(0), loot: getLoot(3) },
    12: { sector: getSector(1), loot: getLoot(0) },
    13: { sector: getSector(2), loot: getLoot(1) },
    14: { sector: getSector(3), loot: getLoot(2) },
    15: { sector: getSector(4), loot: getLoot(3) },
    16: { sector: getSector(5), loot: getLoot(0) },
    17: { sector: getSector(6), loot: getLoot(1) },
    18: { sector: getSector(7), loot: getLoot(2) },
    19: { sector: getSector(8), loot: getLoot(3) },
    20: { sector: getSector(9), loot: getLoot(0) },
    21: { sector: getSector(10), loot: getLoot(1) },

    22: { sector: getSector(0), loot: getLoot(2) },
    23: { sector: getSector(1), loot: getLoot(3) },
    24: { sector: getSector(2), loot: getLoot(0) },
    25: { sector: getSector(3), loot: getLoot(1) },
    26: { sector: getSector(4), loot: getLoot(2) },
    27: { sector: getSector(5), loot: getLoot(3) },
    28: { sector: getSector(6), loot: getLoot(0) },
    29: { sector: getSector(7), loot: getLoot(1) },
    30: { sector: getSector(8), loot: getLoot(2) },
    31: { sector: getSector(9), loot: getLoot(3) },
    32: { sector: getSector(10), loot: getLoot(0) },

    33: { sector: getSector(0), loot: getLoot(1) },
    34: { sector: getSector(1), loot: getLoot(2) },
    35: { sector: getSector(2), loot: getLoot(3) },
    36: { sector: getSector(3), loot: getLoot(0) },
    37: { sector: getSector(4), loot: getLoot(1) },
    38: { sector: getSector(5), loot: getLoot(2) },
    39: { sector: getSector(6), loot: getLoot(3) },
    40: { sector: getSector(7), loot: getLoot(0) },
    41: { sector: getSector(8), loot: getLoot(1) },
    42: { sector: getSector(9), loot: getLoot(2) },
    43: { sector: getSector(10), loot: getLoot(3) },
  },
  zeroHour: {
    0: { burn: 'Void' },
    1: { burn: 'Arc' },
    2: { burn: 'Solar' },
  },
  grandMaster: {
    0: { name: 'Exodus Crash', recordHash: 3505377076, activityHash: 54961125 },
    1: { name: 'The Glassway', recordHash: 3250442757, activityHash: 4197461112 },
    2: { name: 'The Scarlet Keep', recordHash: 3426594834, activityHash: 3449817631 },
    3: { name: 'Broodhold', recordHash: 1445611700, activityHash: 89113250 },
    4: { name: 'The Inverted Spire', recordHash: 164162423, activityHash: 281497220 },
    5: { name: 'The Disgraced', recordHash: 3694149830, activityHash: 3381711459 },
  },
  wellSpring: {
    0: { activity: 201280774, boss: 'Attack - Golmag (Warden)', loot: 'Come to Pass' },
    1: { activity: 2948928434, boss: 'Defend - Vezuul (Lightflayer)', loot: 'Tarnation' },
    2: { activity: 54269080, boss: "Attack - Bor'gong (Warden)", loot: 'Fel Taradiddle' },
    3: { activity: 3333680772, boss: 'Defend - Zeerik (Lightflayer)', loot: "Father's Sins" },
  },
};

const dailyCycleInfo = (type) => {
  const time = new Date().getTime();
  for (var cycle in cycleInfo.cycle) {
    cycleInfo.elapsed[cycle] = time - cycleInfo.epoch[cycle];
    cycleInfo.week[cycle] = Math.floor((cycleInfo.elapsed[cycle] / msPerWk) % cycleInfo.cycle[cycle]);
    cycleInfo.day[cycle] = Math.floor((cycleInfo.elapsed[cycle] / msPerDay) % cycleInfo.cycle[cycle]);
  }
  switch (type) {
    case 'lostsector': {
      return rotations.lostSector[cycleInfo.day.lostSector];
    }
    case 'wellspring': {
      return rotations.wellSpring[cycleInfo.day.wellSpring];
    }
    default:
      return null;
  }
};

const weeklyCycleInfo = (type) => {
  const time = new Date().getTime();
  for (var cycle in cycleInfo.cycle) {
    cycleInfo.elapsed[cycle] = time - cycleInfo.epoch[cycle];
    cycleInfo.week[cycle] = Math.floor((cycleInfo.elapsed[cycle] / msPerWk) % cycleInfo.cycle[cycle]);
    cycleInfo.day[cycle] = Math.floor((cycleInfo.elapsed[cycle] / msPerDay) % cycleInfo.cycle[cycle]);
  }
  switch (type) {
    case 'zeroHour': {
      return rotations.zeroHour;
    }
    case 'grandMaster': {
      return rotations.grandMaster[cycleInfo.week.grandMaster];
    }
  }
};

//Use: weeklyCycleInfo().zerohour[cycleInfo.week.zerohour].burn;
//Use: dailyCycleInfo().lostSector[cycleInfo.day.lostSector];

module.exports = { dailyCycleInfo, weeklyCycleInfo };
