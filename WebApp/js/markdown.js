document.addEventListener('DOMContentLoaded', function () {
  const languageDivs = document.querySelectorAll('div[class^="language-"]');

  const copyContent = `&#128203;copy code`;
  // 为每个代码片段添加复制图标
  languageDivs.forEach(languageDiv => {
    const language = languageDiv.className.split(' ')[0].split('-')[1];

    const codeActionBar = document.createElement('div');
    codeActionBar.classList.add('code-action-bar', 'flex', 'justify-between');

    codeActionBar.innerHTML = `<span>${language}</span><span class="copy-icon">${copyContent}</span>`;
    languageDiv.parentNode.insertBefore(codeActionBar, languageDiv);

    const copyIcon = codeActionBar.querySelector('.copy-icon');
    // 点击复制图标时复制代码片段到剪贴板
    copyIcon.addEventListener('click', () => {
      // 复制代码片段的内容到剪贴板
      // get pre tag element innertext from codeSnippet
      const textToCopy = languageDiv.querySelector('pre').innerText;
      navigator.clipboard.writeText(textToCopy)
        .then(() => {
          // 复制成功后将图标更改为对号
          copyIcon.innerHTML = '&#10003 copied!';

          // 2秒后恢复图标为原始状态
          setTimeout(() => {
            copyIcon.innerHTML = copyContent;
          }, 1500);
        })
        .catch(err => {
          console.error('Failed to copy: ', err);
        });

    });
  });

});