/*  ----------------------------------------------------------------------------
 *  Author:     Burak Göncü
 *  E-Mail:     bmgoncu@gmail.com
 *  ----------------------------------------------------------------------------
 *  Description:
 */

using System;
using System.Collections;
using UnityEngine;

public class AsyncUtils : SingletonComponent<AsyncUtils> {

    /// <summary>
    /// Executes the callback on NextFrame.
    /// </summary>
    /// <param name="action">Callback</param>
    public void DeferToNextFrame(Action action) {
        StartCoroutine(CallNextFrameCoroutine(action));
    }

    /// <summary>
    /// Deffers the callback by the specified delay.
    /// </summary>
    /// <param name="action">Callback</param>
    /// <param name="seconds">Delay</param>
    public void SetDeferredAction(Action action, float seconds) {
        StartCoroutine(DeferredAction(action, seconds)); 
    }

    /// <summary>
    /// Creates a repeating coroutine for the given callback.
    /// </summary>
    /// <param name="action">Callback</param>
    /// <param name="interval">Period</param>
    /// <param name="delay">Delay</param>
    /// <returns></returns>
    public Coroutine SetRepeatingAction(Action action, float interval, float delay = 0f)
    {
        return StartCoroutine(RepeatingAction(action, interval,delay));
    }

    private IEnumerator RepeatingAction(Action action, float interval,float delay)
    {
        if (delay > 0){
            yield return new WaitForSeconds(delay);
        }
        while (true)
        {
            action();
            yield return new WaitForSeconds(interval);
        }
    }

    protected IEnumerator DeferredAction (Action action, float seconds) {
        yield return new WaitForSeconds(seconds);
        action();

    }

    protected IEnumerator CallNextFrameCoroutine(Action action) {
        yield return new WaitForEndOfFrame();
        action();
    }

    
}

