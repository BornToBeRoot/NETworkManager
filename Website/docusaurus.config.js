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
    "A powerful open-source tool for managing networks and troubleshooting network problems!",
  favicon: "img/favicon.ico",

  url: "https://borntoberoot.net",
  baseUrl: "/NETworkManager",

  // GitHub pages deployment config.
  organizationName: "BornToBeRoot",
  projectName: "NETworkManager",

  trailingSlash: false,

  onBrokenLinks: "throw",
  onBrokenAnchors: "throw",

  markdown: {
    hooks: {
      onBrokenMarkdownLinks: "throw",
    },
  },

  i18n: {
    defaultLocale: "en",
    locales: ["en"],
  },

  headTags: [
    {
      tagName: "script",
      attributes: {
        type: "application/ld+json",
      },
      innerHTML: JSON.stringify({
        "@context": "https://schema.org",
        "@type": "SoftwareApplication",
        name: "NETworkManager",
        applicationCategory: "NetworkApplication",
        operatingSystem: "Windows 10, Windows 11, Windows Server",
        offers: {
          "@type": "Offer",
          price: "0",
          priceCurrency: "USD",
        },
        description:
          "A powerful open-source tool for managing networks and troubleshooting network problems. Features IP Scanner, Port Scanner, Ping Monitor, Traceroute, DNS Lookup, Remote Desktop, PowerShell, PuTTY (SSH), and more.",
        url: "https://borntoberoot.net/NETworkManager",
        downloadUrl:
          "https://borntoberoot.net/NETworkManager/download",
        author: {
          "@type": "Person",
          name: "BornToBeRoot",
          url: "https://github.com/BornToBeRoot",
        },
        license:
          "https://github.com/BornToBeRoot/NETworkManager/blob/main/LICENSE",
      }),
    },
  ],

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
          blogSidebarCount: "ALL",
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
      image: "img/social-card.png",
      metadata: [
        {
          name: "keywords",
          content:
            "NETworkManager, network manager, network tools, IP scanner, port scanner, ping monitor, traceroute, DNS lookup, remote desktop, SSH, PuTTY, PowerShell, VNC, WiFi analyzer, subnet calculator, SNMP, network troubleshooting, Windows network tool, open source",
        },
        { name: "author", content: "BornToBeRoot" },
      ],
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
            href: "https://github.com/BornToBeRoot/NETworkManager?tab=readme-ov-file#-donate",
            label: "Donate",
            position: "right",
          },
          {
            href: "https://github.com/BornToBeRoot/NETworkManager",
            label: "GitHub",
            position: "right",
          },
        ],
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
                label: "X",
                href: "https://x.com/_BornToBeRoot",
              },
            ],
          },
          {
            title: "Contributing",
            items: [
              {
                label: "GitHub",
                href: "https://github.com/BornToBeRoot/NETworkManager",
              },
              {
                label: "Transifex",
                href: "https://app.transifex.com/BornToBeRoot/NETworkManager/",
              },
              {
                label: "Code of Conduct",
                href: "https://github.com/BornToBeRoot/NETworkManager/blob/main/CODE_OF_CONDUCT.md",
              },
            ],
          },
        ],
        copyright: `Copyright © ${new Date().getFullYear()} BornToBeRoot and Contributors. Built with Docusaurus.`,
      },
      prism: {
        theme: prismThemes.github,
        darkTheme: prismThemes.dracula,
        additionalLanguages: ["csharp", "json", "powershell"],
      },
    }),

  plugins: [
    [
      // Custom redirects
      "@docusaurus/plugin-client-redirects",
      {
        redirects: [
          {
            // Redirect latest changelog to the newest version
            from: ["/docs/changelog/latest"],
            to: "/docs/changelog/2026-2-22-0",
          },
          // Redirect pre-versions to the corresponding new versions
          {
            from: [              
              "/docs/changelog/2026-3-4-0",
              "/docs/changelog/2026-4-30-0",
            ],
            to: "/docs/changelog/next-release",
          },
          {
            from: [
              "/docs/changelog/2025-11-1-0",
              "/docs/changelog/2025-11-30-0",
              "/docs/changelog/2025-12-13-0",
              "/docs/changelog/2025-12-20-0",
              "/docs/changelog/2025-12-28-0",
              "/docs/changelog/2026-2-19-0",
            ],
            to: "/docs/changelog/2026-2-22-0",
          },
          {
            from: [
              "/docs/changelog/2025-6-13-0",
              "/docs/changelog/2025-7-9-0",
            ],
            to: "/docs/changelog/2025-8-10-0",
          },
        ],
      },
    ],
  ],
};

export default config;
