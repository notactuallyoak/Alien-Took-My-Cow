using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenuFlicker : MonoBehaviour
{
    private float shortBeep = 0.2f;
    private float longBeep = 0.5f;
    private float pauseGap = 0.15f;
    private float letterGap = 5f;

    private Image image;


    private void Start()
    {
        image = GetComponent<Image>();
        StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        yield return new WaitForSeconds(5f); // initial

        while (true)
        {
            yield return StartCoroutine(Flash(image, shortBeep));
            yield return StartCoroutine(Flash(image, shortBeep));
            yield return StartCoroutine(Flash(image, shortBeep));
            yield return StartCoroutine(Flash(image, shortBeep));
            yield return new WaitForSeconds(letterGap);

            yield return StartCoroutine(Flash(image, shortBeep));
            yield return new WaitForSeconds(letterGap);

            yield return StartCoroutine(Flash(image, shortBeep));
            yield return StartCoroutine(Flash(image, longBeep));
            yield return StartCoroutine(Flash(image, shortBeep));
            yield return StartCoroutine(Flash(image, shortBeep));
            yield return new WaitForSeconds(letterGap);

            yield return StartCoroutine(Flash(image, shortBeep));
            yield return StartCoroutine(Flash(image, longBeep));
            yield return StartCoroutine(Flash(image, shortBeep));
            yield return StartCoroutine(Flash(image, shortBeep));
            yield return new WaitForSeconds(letterGap);

            yield return StartCoroutine(Flash(image, longBeep));
            yield return StartCoroutine(Flash(image, longBeep));
            yield return StartCoroutine(Flash(image, longBeep));
            yield return new WaitForSeconds(letterGap);
        }
    }

    private IEnumerator Flash(Image img, float beepDuration)
    {
        img.color = new Color(0.8f, 0.8f, 0.8f); ;
        yield return new WaitForSeconds(beepDuration);
        img.color = new Color(1f, 1f, 1f);

        yield return new WaitForSeconds(pauseGap);
    }
}
