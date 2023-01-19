import {
  AllDestinyManifestComponents,
  DestinyCollectibleDefinition,
  DestinyInventoryItemDefinition,
  DestinyMetricDefinition,
  DestinyPresentationNodeDefinition,
  DestinyRecordDefinition,
} from 'bungie-api-ts/destiny2';
import { ErrorHandler } from './error.handler';
import * as Log from './log.handler';
import * as Database from './database.functions';
import * as APIRequest from './api.handler';
import fs from 'fs';
import dotenv from 'dotenv';

dotenv.config();

let ManifestDefinitions = [
  'DestinyActivityDefinition',
  'DestinyActivityModeDefinition',
  'DestinyActivityModifierDefinition',
  'DestinyCollectibleDefinition',
  'DestinyPresentationNodeDefinition',
  'DestinyRecordDefinition',
  'DestinyInventoryItemDefinition',
  'DestinyInventoryBucketDefinition',
  'DestinyObjectiveDefinition',
  'DestinyProgressionDefinition',
  'DestinyTalentGridDefinition',
  'DestinyVendorDefinition',
  'DestinyMetricDefinition',
];
let Manifest: Partial<AllDestinyManifestComponents> = [] as any;
let ManifestVersion = null;

export const startUp = () => {
  checkManifestUpdate();
};

export const checkManifestMounted = () => {
  if (Object.keys(Manifest).length > 0) {
    return true;
  } else {
    return false;
  }
};

export const getManifest = () => {
  return Manifest;
};

export const getManifestVersion = () => {
  return ManifestVersion;
};

export const getManifestItemByName = (name) => {
  let searchResults = Object.values(Manifest.DestinyInventoryItemDefinition).filter(
    (e) => e.displayProperties.name.toUpperCase() === name.toUpperCase()
  );
  let filterCollectibles = searchResults.filter((e) => e.collectibleHash);
  return filterCollectibles[0];
};

export const searchForCollectible = (name: string) => {
  try {
    let searchResults = Object.values(Manifest.DestinyCollectibleDefinition).filter((e) =>
      e.displayProperties.name.toUpperCase().includes(name.toUpperCase())
    );
    return searchResults.slice(0, 25);
  } catch (err) {
    return [];
  }
};

export const getCurrentSeasonGrandmasters: (conquerorPresentationNodeSealHash: string) => DestinyRecordDefinition[] = (
  conquerorPresentationNodeSealHash: string
) => {
  let presentationNode = Manifest.DestinyPresentationNodeDefinition[
    conquerorPresentationNodeSealHash
  ] as DestinyPresentationNodeDefinition;
  let records = presentationNode.children.records.map(
    (x) => Manifest.DestinyRecordDefinition[x.recordHash] as DestinyRecordDefinition
  );
  return records.filter((x) => x.displayProperties.name.includes('Grandmaster:'));
};

export const searchForTitle = (name?: string) => {
  const legacySealsNode = Manifest.DestinyPresentationNodeDefinition[1881970629];
  const legacysealsParents = legacySealsNode.children.presentationNodes.map((e) => {
    return e.presentationNodeHash;
  });
  const sealsNode = Manifest.DestinyPresentationNodeDefinition[616318467];
  const sealsParents = sealsNode.children.presentationNodes.map((e) => {
    return e.presentationNodeHash;
  });
  let seals = sealsParents.map((e) => {
    return Manifest.DestinyPresentationNodeDefinition[e].completionRecordHash;
  });
  let legacySeals = legacysealsParents.map((e) => {
    return Manifest.DestinyPresentationNodeDefinition[e].completionRecordHash;
  });
  let gildedSeals = seals.map((e) => {
    return Manifest.DestinyRecordDefinition[e]?.titleInfo?.gildingTrackingRecordHash;
  });
  let legacyGildedSeals = legacySeals.map((e) => {
    return Manifest.DestinyRecordDefinition[e]?.titleInfo?.gildingTrackingRecordHash;
  });
  let allSeals = seals.concat(legacySeals);
  let allGildedSeals = gildedSeals.concat(legacyGildedSeals.filter((e) => e !== undefined));
  let allSealsCombined = allSeals.concat(allGildedSeals.filter((e) => e !== undefined));
  let allSealRecords = allSealsCombined.map((hash) => Manifest.DestinyRecordDefinition[hash]);

  try {
    if (!name) {
      return allSealRecords.slice(0, 25);
    } else {
      let searchResults = Object.values(allSealRecords).filter((e) =>
        e.displayProperties.name.toUpperCase().includes(name.toUpperCase()) || 
        (e.titleInfo.titlesByGender['Male'].toUpperCase().includes(name.toUpperCase()) && e.displayProperties.description)
      );
      return searchResults.slice(0, 25);
    }
  } catch (err) {
    return [];
  }
};

export const searchPresentationNodeThatIsLinkedToRecord = (hash: number) => {
  const legacySealsNode = Manifest.DestinyPresentationNodeDefinition[1881970629];
  const legacysealsParents = legacySealsNode.children.presentationNodes.map((e) => {
    return e.presentationNodeHash;
  });
  const sealsNode = Manifest.DestinyPresentationNodeDefinition[616318467];
  const sealsParents = sealsNode.children.presentationNodes.map((e) => {
    return e.presentationNodeHash;
  });
  let seals = sealsParents.map((e) => {
    return Manifest.DestinyPresentationNodeDefinition[e];
  });
  let legacySeals = legacysealsParents.map((e) => {
    return Manifest.DestinyPresentationNodeDefinition[e];
  });
  let allSeals = seals.concat(legacySeals);

  try {
    let searchResults = Object.values(allSeals).filter((e) => e.completionRecordHash === hash);
    return searchResults.slice(0, 25);
  } catch (err) {
    return [];
  }
};

export const getCollectible = (hash: string): DestinyCollectibleDefinition =>
  Manifest.DestinyCollectibleDefinition[hash];

export const getRecord = (hash: string): DestinyRecordDefinition => Manifest.DestinyRecordDefinition[hash];

export const getItem = (hash: string): DestinyInventoryItemDefinition => Manifest.DestinyInventoryItemDefinition[hash];

export const getMetric = (hash: string): DestinyMetricDefinition => Manifest.DestinyMetricDefinition[hash];

export const getPresentationNode = (hash: string): DestinyPresentationNodeDefinition =>
  Manifest.DestinyPresentationNodeDefinition[hash];

export const getManifestItemByHash = (hash) => {
  return Manifest.DestinyInventoryItemDefinition[hash];
};

export const getManifestItemByCollectibleHash = (collectibleHash) => {
  let collectible = Manifest.DestinyCollectibleDefinition[collectibleHash];
  let item = Manifest.DestinyInventoryItemDefinition[collectible?.itemHash];
  return item ? item : collectible;
};

export const getManifestTitleByName = (name: string) => {
  const legacySealsNode = getManifest().DestinyPresentationNodeDefinition[1881970629];
  const legacysealsParents = legacySealsNode.children.presentationNodes.map((e) => {
    return e.presentationNodeHash;
  });
  const sealsNode = getManifest().DestinyPresentationNodeDefinition[616318467];
  const sealsParents = sealsNode.children.presentationNodes.map((e) => {
    return e.presentationNodeHash;
  });
  let seals = sealsParents.map((e) => {
    return getManifest().DestinyRecordDefinition[
      getManifest().DestinyPresentationNodeDefinition[e].completionRecordHash
    ];
  });
  let legacySeals = legacysealsParents.map((e) => {
    return getManifest().DestinyRecordDefinition[
      getManifest().DestinyPresentationNodeDefinition[e].completionRecordHash
    ];
  });
  let gildedSeals = seals.map((e) => {
    return getManifest().DestinyRecordDefinition[
      getManifest().DestinyRecordDefinition[e.hash].titleInfo.gildingTrackingRecordHash
    ];
  });
  let legacyGildedSeals = legacySeals.map((e) => {
    return getManifest().DestinyRecordDefinition[
      getManifest().DestinyRecordDefinition[e.hash].titleInfo.gildingTrackingRecordHash
    ];
  });
  let allSeals = seals.concat(legacySeals);
  let allGildedSeals = gildedSeals.concat(legacyGildedSeals.filter((e) => e !== undefined));
  let allSealsCombined = allSeals.concat(allGildedSeals.filter((e) => e !== undefined));

  return allSealsCombined.filter((e) => e.titleInfo.titlesByGender['Male'].toUpperCase() === name.toUpperCase());
};

export const storeManifest = async function StoreManifest() {
  await Database.getManifestVersion((isError, isFound, data) => {
    if (!isError) {
      if (isFound) {
        ManifestVersion = data[0].version;
        for (let definition of ManifestDefinitions) {
          try {
            Manifest[definition] = JSON.parse(fs.readFileSync(`src/manifest/${definition}.json`).toString());
          } catch (err) {
            // TODO: Manaually grab and install new manifest file
            // Log.SaveLog("Frontend", "Error", 'Failed to read manifest file, re-downloading...');
            break;
          }
        }
      } else {
        ErrorHandler('High', `Failed to get manifest version as there is not one in the database.`);
      }
    } else {
      ErrorHandler('High', `Failed to get manifest version: ${data}`);
    }
  });
};

//Backend manages this check, it runs every 10 minutes. Neither frontend nor Express should interact with this function to avoid multiple updates.
export const checkManifestUpdate = async function CheckManifestUpdate(location?: string) {
  if (await checkManifestFilesExist()) {
    await Database.getManifestVersion(function GetOldManifestVersion(isError, isFound, version) {
      if (!isError) {
        if (isFound) {
          if (location === 'frontend') {
            //If frontend, check if manifest has been updated and restore.
            if (getManifestVersion() !== version[0]?.version) {
              Log.SaveLog(
                'Frontend',
                'Info',
                `Frontend: Manifest is different re-storing... (${getManifestVersion()}) to (${version[0]?.version})`
              );
              storeManifest();
            }
          } else {
            //If backend, check if manifest needs an update
            APIRequest.GetManifestVersion(function GetNewManifestVersion(isError, newVersion) {
              if (!isError) {
                if (version[0]?.version !== newVersion.Response.version) {
                  Log.SaveLog(
                    'Backend',
                    'Info',
                    `Backend: Manifest is different updating... (${version[0]?.version}) to (${newVersion.Response.version})`
                  );
                  updateManifest(false);
                } else {
                  if (!checkManifestMounted()) {
                    storeManifest();
                  }
                }
              } else {
                ErrorHandler('High', `Failed to get new manifest version: ${newVersion}`);
              }
            });
          }
        } else {
          updateManifest(false);
        }
      } else {
        ErrorHandler('High', `Failed to get manifest version: ${version}`);
      }
    });
  } else {
    if (location === 'frontend') {
    } else {
      Log.SaveLog('Backend', 'Info', `Manifest is missing, downloading...`);
      updateManifest(false);
    }
  }
};

export const checkManifestFilesExist = async (): Promise<boolean> => {
  for (let definition of ManifestDefinitions) {
    if (!fs.existsSync(`src/manifest/${definition}.json`)) {
      console.log(`Manifest not found: src/manifest/${definition}.json`);
      return false;
    }
  }
  return true;
};

export const updateManifest = async function UpdateManifest(retried) {
  APIRequest.GetManifestVersion(function GetManifestVersion(isError, data) {
    const manifest = data.Response;
    const manifestVersion = manifest.version;
    if (!isError) {
      Promise.all([
        ...ManifestDefinitions.map((def) =>
          APIRequest.GetManifest(manifest.jsonWorldComponentContentPaths['en'][def] + '?123kool')
        ),
      ])
        .then(async (values: any) => {
          let didValidateError = false;
          let didError = false;

          //Validate the values
          values.map((e: any, index) => {
            if (Object.keys(e.Data).length > 0) {
            } else {
              didValidateError = true;
            }
          });

          //If no validation errors then write the files
          if (!didValidateError) {
            values.map((value, index) => {
              fs.writeFile(
                `src/manifest/${ManifestDefinitions[index]}.json`,
                JSON.stringify(values[index].Data),
                function (err) {
                  if (err) {
                    ErrorHandler('High', err);
                    didError = true;
                  }
                }
              );
            });
          } else {
            ErrorHandler('High', 'Failed to update Manifest, Validation Error');
          }

          //Update version in database
          if (!didError && !didValidateError) {
            Database.updateManifestVersion(
              { name: 'Version', version: manifestVersion },
              function UpdateManifestVersion(isError, severity, data) {
                if (isError) {
                  ErrorHandler(severity, data);
                } else {
                  storeManifest();
                }
              }
            );
          } else {
            ErrorHandler(
              'High',
              `Failed to update Manifest - Validation: ${didValidateError ? 'failed' : 'passed'}, WriteError: ${
                didError ? 'failed' : 'passed'
              } - ${retried ? 'Not Retrying' : 'Retrying'}`
            );
            if (!retried) {
              updateManifest(true);
            }
          }
        })
        .catch((error) => {
          ErrorHandler('High', error);
        });
    } else {
      ErrorHandler('High', data);
    }
  });
};

export const verifyManifest = (callback) => {
  let validation = Object.keys(Manifest).map((e) => {
    if (Manifest[e]) {
      if (Object.keys(Manifest[e]).length > 0) {
        return { component: e, passed: true };
      } else {
        return { component: e, passed: false };
      }
    } else {
      return { component: e, passed: false };
    }
  });
  callback(validation);
};
