using UnityEngine;

/// <summary>
/// ShowInfo 是全场景共享的一块 UI,多个脚本会先后写入消息。
/// 用递增令牌区分"当前显示的消息属于谁",防止某个脚本延迟执行的
/// 隐藏逻辑关掉别的脚本后来展示的新消息。
/// </summary>
public static class ScreenMessageGate
{
    private static uint generation = 0;

    /// <summary>当前代数(最新一次 Show 的令牌)。</summary>
    public static uint Current => generation;

    /// <summary>展示消息前调用,返回本次消息的令牌。</summary>
    public static uint Begin() => ++generation;

    /// <summary>延迟隐藏时调用:只有令牌仍是最新时才允许隐藏。</summary>
    public static bool CanHide(uint token) => generation == token;

    /// <summary>
    /// 展示共享 UI 时调用:激活并重置其 CountdownEvent 倒计时,
    /// 避免新消息被上一次显示留下的旧倒计时提前隐藏。
    /// </summary>
    public static void Arm(GameObject ui)
    {
        if (ui == null) return;
        ui.SetActive(true);
        var cd = ui.GetComponent<CountdownEvent>();
        if (cd != null)
        {
            cd.Reset();
            cd.StartClock();
        }
    }

    /// <summary>同 Arm,并允许覆盖本次倒计时时长(秒)。</summary>
    public static void Arm(GameObject ui, float durationOverride)
    {
        if (ui == null) return;
        ui.SetActive(true);
        var cd = ui.GetComponent<CountdownEvent>();
        if (cd != null)
        {
            cd.Reset();
            cd.clock = durationOverride;
            cd.StartClock();
        }
    }
}
