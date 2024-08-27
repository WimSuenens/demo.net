function write(element, text, i = 0) {
  if (i === 0) element.textContent = "";
  element.textContent += text[i];
  if (i === (text.length - 1)) return;
  setTimeout(() => write(element, text, i + 1), 20);
}

export function writeText(id, text) {
  const element = document.getElementById(id);
  if (!element) return;
  return write(element, text);
}