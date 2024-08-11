export function getScrollTopPercent(element: HTMLElement)
{
  const h = element,
    b = element,
    st = "scrollTop",
    sh = "scrollHeight";

  return (h[st] || b[st]) / ((h[sh] || b[sh]) - h.clientHeight);
}
