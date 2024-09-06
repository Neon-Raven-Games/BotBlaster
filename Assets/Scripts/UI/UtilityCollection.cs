using System.Collections.Generic;
using RNGNeeds;
using UnityEngine;

namespace UI
{
    public class UtilityCollection : MonoBehaviour
    {
        [SerializeField] private List<UpgradeHoverUI> upgradeHoverUIs; 
        private Dictionary<UtilityUpgrade, UpgradeHoverUI> _utilityToUpgradeHoverUI;
        private UtilityUpgrade _currentUtility;
        private ProbabilityList<UtilityUpgrade> _utilityProbabilityList;
        private void PopulateLists()
        {
            if (_utilityProbabilityList != null && _utilityToUpgradeHoverUI != null) return;
            _utilityProbabilityList = new ProbabilityList<UtilityUpgrade>();
            _utilityToUpgradeHoverUI = new Dictionary<UtilityUpgrade, UpgradeHoverUI>();
            foreach (var upgradeHoverUI in upgradeHoverUIs)
            {
                _utilityToUpgradeHoverUI.Add(upgradeHoverUI.utilityUpgrade, upgradeHoverUI);
                _utilityProbabilityList.AddItem(upgradeHoverUI.utilityUpgrade, 50);
                upgradeHoverUI.gameObject.SetActive(false);
            }
        }
        
        private void OnEnable()
        {
            PopulateLists();
            foreach (var upgradeHoverUI in upgradeHoverUIs)
                upgradeHoverUI.gameObject.SetActive(false);
            RollUtility();
        }
        
        private void RollUtility()
        {
            _currentUtility = _utilityProbabilityList.PickValue();
            _utilityToUpgradeHoverUI[_currentUtility].gameObject.SetActive(true);
        }
        
        private void OnDisable()
        {
            _utilityToUpgradeHoverUI[_currentUtility].gameObject.SetActive(false);
        }
    }
}