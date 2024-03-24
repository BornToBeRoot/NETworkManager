// @ts-check
// `@type` JSDoc annotations allow editor autocompletion and type checking
// (when paired with `@ts-check`).
// There are various equivalent ways to declare your Docusaurus config.
// See: https://docusaurus.io/docs/api/docusaurus-config

import { themes as prismThemes } from "prism-react-renderer";

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: "NETworkManager",
  tagline:
    "A powerful tool for managing networks and troubleshoot network problems!",
  favicon: "img/favicon.ico",

  url: "https://borntoberoot.net",
  baseUrl: "/NETworkManager",

  // GitHub pages deployment config.
  organizationName: "BornToBeRoot",
  projectName: "NETworkManager",

  trailingSlash: false,

  onBrokenLinks: "throw",
  onBrokenMarkdownLinks: "throw",

  i18n: {
    defaultLocale: "en",
    locales: ["en"],
  },

  presets: [
    [
      "classic",
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          sidebarPath: "./sidebars.js",
          editUrl:
            "https://github.com/bornToBeRoot/NETworkManager/tree/main/Website/",
        },
        blog: {
          showReadingTime: true,
          editUrl:
            "https://github.com/bornToBeRoot/NETworkManager/tree/main/Website/",
        },
        theme: {
          customCss: "./src/css/custom.css",
        },
      }),
    ],
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      // Replace with your project's social card
      image: "img/docusaurus-social-card.jpg",
      navbar: {
        title: "NETworkManager",
        logo: {
          alt: "NETworkManager Logo",
          src: "img/logo.svg",
        },
        items: [
          {
            to: "/download",
            label: "Download",
            position: "left",
          },
          {
            type: "docSidebar",
            sidebarId: "docsSidebar",
            position: "left",
            label: "Docs",
          },
          {
            to: "/blog",
            label: "Blog",
            position: "left",
          },
          {
            href: "https://github.com/BornToBeRoot/NETworkManager",
            label: "GitHub",
            position: "right",
          },
        ],
      },
      themeConfig: {
        tableOfContents: {
          minHeadingLevel: 1,
          maxHeadingLevel: 5,
        },
      },
      footer: {
        style: "dark",
        links: [
          {
            title: "Quick Links",
            items: [
              {
                label: "Download",
                to: "/download",
              },
              {
                label: "Docs",
                to: "/docs/introduction",
              },
              {
                label: "Blog",
                to: "/blog",
              },
            ],
          },
          {
            title: "Community",
            items: [
              {
                label: "Twitter",
                href: "https://twitter.com/_BornToBeRoot",
              },
            ],
          },
          {
            title: "Source Code",
            items: [
              {
                label: "GitHub",
                href: "https://github.com/BornToBeRoot/NETworkManager",
              },
            ],
          },
        ],
        copyright: `Copyright © ${new Date().getFullYear()} BornToBeRoot and Contributors. Built with Docusaurus.`,
      },
      prism: {
        theme: prismThemes.github,
        darkTheme: prismThemes.dracula,
      },
    }),
};

export default config;
