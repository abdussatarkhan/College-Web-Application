import urllib.request
import os
import shutil
import re

docs_dir = 'docs'
os.makedirs(docs_dir, exist_ok=True)

# 1. Fetch live rendered HTML from local running app
with urllib.request.urlopen('http://localhost:5161') as resp:
    html = resp.read().decode('utf-8')

# 2. Adjust paths to relative paths for GitHub Pages root
html = html.replace('href="/css/', 'href="./css/')
html = html.replace('src="/css/', 'src="./css/')
html = html.replace('href="/images/', 'href="./images/')
html = html.replace('src="/images/', 'src="./images/')
html = html.replace('src="/videos/campus-drone.mp4"', 'src="https://raw.githubusercontent.com/abdussatarkhan/College-Web-Application/main/College%20Web%20Application/wwwroot/videos/campus-drone.mp4"')
html = html.replace('href="/lib/', 'href="./lib/')
html = html.replace('src="/lib/', 'src="./lib/')
html = html.replace('poster="/images/', 'poster="./images/')
html = html.replace('href="/"', 'href="./"')

# Add interactive demo modal for portal buttons
demo_modal = """
<!-- Interactive Demo Modal for ERP Links -->
<div class="modal fade" id="erpDemoModal" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content shadow border-0">
            <div class="modal-header text-white" style="background-color: #063B13; border-bottom: 3px solid #D4AF37;">
                <h6 class="modal-title fw-bold"><i class="bi bi-shield-lock-fill me-2 text-warning"></i>GDC Madyan Swat &bull; ERP Portal Demo</h6>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body p-4 text-center">
                <img src="./images/gdc-madyan-logo.png" alt="GDC Madyan" style="width: 70px; height: 70px; object-fit: contain;" class="mb-3" />
                <h5 class="fw-bold text-dark">Enterprise CMS & ERP Portal</h5>
                <p class="text-secondary small">
                    This is the live GitHub Pages preview of <strong>Government Degree College Madyan Swat</strong>.
                    The complete backend with <strong>ASP.NET Core 10.0, EF Core, 75% Attendance Compliance Engine, Examination DMC Generator</strong>, and 6 security roles is available in the GitHub repository.
                </p>
                <div class="alert alert-success py-2 small fw-semibold text-start">
                    <i class="bi bi-check-circle-fill me-1"></i> Full C# Backend Code: <a href="https://github.com/abdussatarkhan/College-Web-Application" target="_blank" class="fw-bold text-success">GitHub Repository</a>
                </div>
            </div>
            <div class="modal-footer bg-light py-2">
                <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">Close</button>
                <a href="https://github.com/abdussatarkhan/College-Web-Application" target="_blank" class="btn btn-success btn-sm fw-bold">View Source Code</a>
            </div>
        </div>
    </div>
</div>
<script>
document.addEventListener("DOMContentLoaded", function() {
    var erpLinks = document.querySelectorAll('a[href^="/Account/Login"], a[href^="/Admissions/Apply"], a[href^="/Admissions/MeritLists"], a[href^="/Faculty/Attendance"], a[href^="/Admin/ExamControllerOffice"]');
    erpLinks.forEach(function(el) {
        el.addEventListener("click", function(e) {
            e.preventDefault();
            var modal = new bootstrap.Modal(document.getElementById('erpDemoModal'));
            modal.show();
        });
    });
});
</script>
</body>
"""

html = html.replace('</body>', demo_modal)

# Write index.html
with open(os.path.join(docs_dir, 'index.html'), 'w', encoding='utf-8') as f:
    f.write(html)
print('docs/index.html generated successfully!')

# 3. Copy assets into docs/ (excluding duplicate 18MB video since it streams directly from repo)
wwwroot = 'College Web Application/wwwroot'
for folder in ['css', 'images', 'lib']:
    src = os.path.join(wwwroot, folder)
    dst = os.path.join(docs_dir, folder)
    if os.path.exists(src):
        if os.path.exists(dst):
            shutil.rmtree(dst)
        shutil.copytree(src, dst)
        print(f'Copied {folder} -> docs/{folder}')

# Create .nojekyll so GitHub Pages does not ignore any folder starting with underscore or special names
with open(os.path.join(docs_dir, '.nojekyll'), 'w') as f:
    f.write('')
print('docs/.nojekyll created!')
