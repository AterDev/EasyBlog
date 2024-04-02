document.addEventListener('DOMContentLoaded', function () {
  document.getElementById('searchBtn').addEventListener('click', function () {
    var searchText = document.getElementById('searchText').value;
    search(searchText);
  });
  // add keydown event for search
  document.getElementById('searchText').addEventListener('keydown', function (e) {
    if (e.key === 'Enter') {
      var searchText = document.getElementById('searchText').value;
      search(searchText);
    }
  });

  document.getElementById("catalog-list").addEventListener('click', function (e) {
    if (e.target.classList.contains('filter-item')) {
      let catalogName = e.target.dataset.catalog ?? '';
      filterBlogs(catalogName, 'all');
    }
  });

  document.getElementById("date-list").addEventListener('click', function (e) {
    if (e.target.classList.contains('filter-item')) {
      let date = e.target.dataset.date ?? '';
      filterBlogs('all', date);
    }
  });

  // 时间显示
  var dates = document.querySelectorAll('.publish-time');
  dates.forEach(function (date) {
    const dateTime = date.dataset.time;
    date.innerText = timeAgo(new Date(dateTime));
  });

});

let blogs = [];
let allBlogs = [];
let catalogs = [];

let webInfo;

fetch(baseUrl + 'data/blogs.json')
  .then(res => res.json()).then(data => {
    allBlogs = getAllBlogs(data).sort((a, b) => {
      return new Date(b.PublishTime) - new Date(a.PublishTime);
    });
    blogs = allBlogs.slice(0, 50);
    catalogs = data.Children;
  });

fetch(baseUrl + 'data/webinfo.json')
  .then(res => res.json()).then(data => {
    webInfo = data;
  });

function search(key) {
  if (!key) {
    blogs = allBlogs.slice(0, 50);
  } else {
    blogs = allBlogs.filter(blog => blog.Title.toLowerCase().includes(key));
  }
  renderBlogs();
}

function filterBlogs(catalogName, date) {
  if (catalogName != 'all') {
    let catalog = catalogs.find(catalog => catalog.Name == catalogName);
    blogs = catalog.Blogs;
  } else if (date != 'all') {

    blogs = allBlogs.filter(blog => blog.PublishTime.substr(0, 7) == date);
  } else {
    blogs = allBlogs.slice(0, 50);

  }
  renderBlogs();
}

function getAllBlogs(rootCatalog) {
  let blogs = [];
  blogs.push(...rootCatalog.Blogs);
  if (rootCatalog.Children && rootCatalog.Children.length > 0) {
    rootCatalog.Children.forEach(catalog => {
      blogs.push(...getAllBlogs(catalog));
    });
  }
  return blogs;
}

function renderBlogs() {
  let blogList = document.getElementById('blogList');

  blogList.innerHTML = '';
  blogs.forEach(blog => {
    let blogDiv = document.createElement('div');
    blogDiv.className = 'w-100 rounded overflow-hidden shadow-lg dark:bg-neutral-800 my-2';
    let blogInnerDiv = document.createElement('div');
    blogInnerDiv.className = 'px-6 py-3';
    let blogTitleDiv = document.createElement('div');
    blogTitleDiv.className = 'font-bold text-xl mb-2';
    let blogLink = document.createElement('a');
    blogLink.href = './blogs' + blog.Path;
    blogLink.target = '_blank';
    blogLink.className = 'block text-lg py-2 text-neutral-600 hover:text-neutral-800 dark:text-neutral-300 dark:hover:text-neutral-100';
    blogLink.innerText = "📑 " + blog.Title;
    blogTitleDiv.appendChild(blogLink);
    blogInnerDiv.appendChild(blogTitleDiv);
    let blogInfoP = document.createElement('p');
    blogInfoP.className = 'text-neutral-700 text-base dark:text-neutral-300';
    blogInfoP.innerHTML = `👨‍💻 ${webInfo.AuthorName} &nbsp;&nbsp; ⏱️ ${timeAgo(new Date(blog.PublishTime))}`;
    blogInnerDiv.appendChild(blogInfoP);
    blogDiv.appendChild(blogInnerDiv);
    blogList.appendChild(blogDiv);
  });
}

function timeAgo(date) {
  const seconds = Math.floor((new Date() - date) / 1000);
  const intervals = {
    '年': 31536000,
    '月': 2592000,
    '天': 86400,
    '小时': 3600,
    '分钟': 60,
    '秒': 1
  };

  let counter;
  let values = [];
  for (const [unit, secondsPerUnit] of Object.entries(intervals)) {
    counter = Math.floor(seconds / secondsPerUnit);
    values.push([counter, unit]);
  }

  console.log(values);

  for (let i = 0; i < values.length; i++) {
    const [counter, unit] = values[i];
    if (counter > 0) {

      if (unit === '年') {
        // show year and month
        let month = Math.floor((seconds - counter * intervals[unit]) / intervals['月']);
        month = month > 0 ? month + '月' : '';
        return `${counter}${unit}${month}前`;
      }
      if (unit === '月') {
        let day = Math.floor((seconds - counter * intervals[unit]) / intervals['天']);
        day = day > 0 ? day + '天' : '';
        return `${counter}${unit}${day}前`;
      }
      if (unit === '天') {
        lethour = Math.floor((seconds - counter * intervals[unit]) / intervals['小时']);
        hour = hour > 0 ? hour + '小时' : '';
        return `${counter}${unit}${hour}前`;
      }
      return `${counter}${unit}前`;
    }
  }

  return '刚刚';
}
