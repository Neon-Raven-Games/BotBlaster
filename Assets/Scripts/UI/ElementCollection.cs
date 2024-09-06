using System;
using System.Collections.Generic;
using Gameplay.Enemies;
using RNGNeeds;
using UnityEngine;

namespace UI
{
    public class ElementCollection : MonoBehaviour
    {
        [SerializeField] private List<UpgradeHoverUI> upgradeHoverUIs;
        private Dictionary<ElementFlag, UpgradeHoverUI> _elementToUpgradeHoverUI;
        private ProbabilityList<ElementFlag> _elementProbabilityList;
        private ElementFlag _currentElement;
        private void PopulateLists()
        {
            if (_elementProbabilityList != null && _elementToUpgradeHoverUI != null) return;
            _elementProbabilityList = new ProbabilityList<ElementFlag>();
            _elementToUpgradeHoverUI = new Dictionary<ElementFlag, UpgradeHoverUI>();
            foreach (var upgradeHoverUI in upgradeHoverUIs)
            {
                _elementProbabilityList.AddItem(upgradeHoverUI.elementFlag, 50);
                _elementToUpgradeHoverUI.Add(upgradeHoverUI.elementFlag, upgradeHoverUI);
                upgradeHoverUI.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            PopulateLists();
            RollElement();
        }

        private void OnDisable()
        {
            _elementToUpgradeHoverUI[_currentElement].gameObject.SetActive(false);
        }

        private void RollElement()
        {
            _currentElement = _elementProbabilityList.PickValue();
            _elementToUpgradeHoverUI[_currentElement].gameObject.SetActive(true);
        }
    }
}