using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace EventListeners
{
    public class PointsIndicator : MonoBehaviour
    {
        private bool _canMoveUp;

        internal void ShowPoints(int amount)
        {
            gameObject.GetComponentInChildren<Text>().text = $"+{amount}";
            StopCoroutine(DoAnimation());
            StartCoroutine(DoAnimation());
        }

        private IEnumerator DoAnimation()
        {
            _canMoveUp = true;
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }

        private void Update()
        {
            if (!_canMoveUp) return;
            gameObject.transform.localPosition = new Vector2(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y + 10f * Time.deltaTime);
        }
    }
}
