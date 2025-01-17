module.exports = function (context) {
    return {
      name: "docusaurus-plugin-gurubase-widget", // Feel free to change this name
      injectHtmlTags() {
        return {
          postBodyTags: [
            {
              tagName: "script",
              attributes: {
                src: "https://widget.gurubase.io/widget.latest.min.js",
                "data-widget-id": "Zd_w46FaGI0XssTb2Sjivabt0w__rvepD0w1rQ5uKIg",
                "data-text": "Ask AI",
                "data-margins": '{"bottom": "20px", "right": "20px"}',
                "data-light-mode": "false",
                "data-name": "NETworkManager",
                "data-icon-url": "https://borntoberoot.net/NETworkManager/img/logo.svg",
                "data-bg-color": "#00d4aa",
                defer: true,
                id: "guru-widget-id", // Do not change this
              },
            },
          ],
        };
      },
    };
  };
  