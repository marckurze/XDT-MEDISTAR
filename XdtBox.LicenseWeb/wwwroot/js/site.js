document.addEventListener("DOMContentLoaded", () => {
  document.querySelectorAll(".customer-row[data-href]").forEach((row) => {
    const open = () => {
      const href = row.getAttribute("data-href");
      if (href) {
        window.location.href = href;
      }
    };
    const isControl = (target) => target.closest("a, button, input, label, select, textarea, form");

    row.addEventListener("click", (event) => {
      if (!isControl(event.target)) {
        open();
      }
    });

    row.addEventListener("keydown", (event) => {
      if ((event.key === "Enter" || event.key === " ") && !isControl(event.target)) {
        event.preventDefault();
        open();
      }
    });
  });
});
