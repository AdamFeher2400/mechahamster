﻿// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using System.Collections.Generic;

namespace Hamster.States
{
    class LevelSelect : BaseLevelSelect
    {
        private LevelDirectory levelDir;
        public const string LevelDirectoryJson = "LevelList";
        private bool startLevelImmediately = false;
        private int forceLoadLevelIdx;
        private bool startGameplayAfterLoad = false;
        bool doneLoading = false;
        // Called whenever a level is selected in the menu.
        protected override void LoadLevel(int i)
        {
            if (currentLoadedMap == -1)
                currentLoadedMap = i;
            TextAsset json = Resources.Load(levelDir.levels[currentLoadedMap].filename) as TextAsset;
            currentLevel = JsonUtility.FromJson<LevelMap>(json.ToString());
            currentLevel.DatabasePath = null;
            CommonData.gameWorld.DisposeWorld();
            CommonData.gameWorld.SpawnWorld(currentLevel);
        }

        public bool ForceLoadLevel(int i)
        {

            startGameplayAfterLoad = true;
            if (currentLoadedMap == -1)
            {
                mapSelection = i;
                startLevelImmediately = true;
                return false;   //  early bail
            }
            else
            {
                startLevelImmediately = true;
                forceLoadLevelIdx = i;
            }
            return true;
        }
        void InitializeMenu()
        {
            string[] levelNames = new string[levelDir.levels.Count];

            // Generate a list of level names.
            for (int i = 0; i < levelDir.levels.Count; i++)
            {
                levelNames[i] = levelDir.levels[i].name;
            }
            MenuStart(levelNames, StringConstants.BuiltinLevelScreenTitle);

        }
        // Initialization method.  Called after the state is added to the stack.
        public override void Initialize()
        {
            TextAsset json = Resources.Load(LevelDirectoryJson) as TextAsset;
            levelDir = JsonUtility.FromJson<LevelDirectory>(json.ToString());
            if (startLevelImmediately)
            {
                LoadLevel(forceLoadLevelIdx);

            }
            else
            {// this was the original behavior, to let you choose your level.
                InitializeMenu();
            }
        }

        public override void Update()
        {
            base.Update();
            if (levelLoaded)
            {
                if (startGameplayAfterLoad)
                {
                    startGameplayAfterLoad = false;
                    PlayerController.StartGamePlay();   //  we can't do this here because we need some frames to let the level load.
                }
            }
        }
        [System.Serializable]
        public class LevelDirectory
        {
            public LevelDirectory() { }

            public LevelDirectory(List<PremadeLevelEntry> levels)
            {
                this.levels = levels;
            }

            public List<PremadeLevelEntry> levels;
        }


        [System.Serializable]
        public class PremadeLevelEntry
        {
            public string name;
            public string description;
            public string filename;
            public string replay;

            public PremadeLevelEntry() { }

            public PremadeLevelEntry(string name, string description, string filename)
            {
                this.name = name;
                this.description = description;
                this.filename = filename;
            }
        }
    }
}
