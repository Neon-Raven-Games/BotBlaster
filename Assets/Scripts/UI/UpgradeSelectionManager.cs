using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gameplay.Enemies;
using UnityEngine;

namespace UI
{
    public class UpgradeSelectionManager : MonoBehaviour
    {
        [SerializeField] private DevController player;
        [SerializeField] private Transform hmd;
        [SerializeField] private float spawnDistance = 2f;
        [SerializeField] private float movementSpeed;
        [SerializeField] private float rotationSpeed = 8f;
        public async UniTask ShowUpgradeSelection()
        {
            await UniTask.WaitUntil(() =>
            {
                    Debug.Log("Waiting for UI");
                return !gameObject.activeInHierarchy;
            });
        }
        
        private void OnEnable()
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(1f, 0.35f).SetEase(Ease.InOutExpo);
        }
        
        private void Update()
        {
            var targetPos = hmd.transform.position + hmd.forward * spawnDistance;
            targetPos.y = transform.position.y;
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * movementSpeed);
            var direction = hmd.transform.position - transform.position;
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