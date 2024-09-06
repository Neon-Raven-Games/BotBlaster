using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Enemies;
using UnityEngine;

namespace UI
{
    public class UpgradeSelectionManager : MonoBehaviour
    {
        [SerializeField] private DevController player;
        [SerializeField] private float spawnDistance = 2f;

        [SerializeField] private float rotationSpeed = 8f;
        public async UniTask ShowUpgradeSelection()
        {
            await UniTask.WaitUntil(() => !gameObject.activeInHierarchy);
        }
        
        private void OnEnable()
        {
            var position = player.transform.position;
            position += player.transform.forward* spawnDistance;
            position.y = transform.position.y;
            transform.position = position;
            transform.localScale = Vector3.zero;
            transform.DOScale(1f, 0.35f).SetEase(Ease.InOutExpo);
        }

        private void Update()
        {
            var direction = player.transform.position - transform.position;
            direction.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, 
                Quaternion.LookRotation(direction, Vector3.up), Time.deltaTime * rotationSpeed);
        }

        public void UpgradeSelected(UpgradeType upgradeType, ElementFlag elementFlag)
        {
            player.UpgradeSelected(upgradeType, elementFlag);
            gameObject.SetActive(false);
        }

        public void UpgradeSelected(UtilityUpgrade utilityUpgrade)
        {
            player.UpgradeSelected(utilityUpgrade);
            gameObject.SetActive(false);
        }
    }
}